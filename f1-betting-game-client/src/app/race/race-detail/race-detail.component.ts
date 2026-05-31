import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { RaceService } from '../services/race.service';
import { Observable, of, forkJoin } from 'rxjs';
import { switchMap, map, catchError, shareReplay } from 'rxjs/operators';
import { RaceDetailDto } from '../models/race.models';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { PositionDto, RaceResultDto } from '../services/race.service';
import { DurationPipe } from '../shared/duration.pipe';

interface RaceDetailData {
  details: RaceDetailDto;
  odds: Record<number, number>;
}

@Component({
  selector: 'app-race-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, DurationPipe],
  templateUrl: './race-detail.component.html',
  styleUrls: ['./race-detail.component.css'],
})
export class RaceDetailComponent implements OnInit {
  raceDetailData$!: Observable<RaceDetailData>;
  raceResult$!: Observable<RaceResultDto | null>; // <-- Change to Observable

  constructor(
    private activatedRoute: ActivatedRoute,
    private raceService: RaceService,
    private authService: AuthService
  ) {}

  get isLoggedIn$(): Observable<unknown> {
    return this.authService.currentUser;
  }

  ngOnInit(): void {
    // 1. Fetch the main detail data (No changes here)
    this.raceDetailData$ = this.activatedRoute.paramMap.pipe(
      map((params: ParamMap) => params.get('id')!),
      switchMap((raceId: string) => {
        const id = Number(raceId);
        return forkJoin({
          details: this.raceService.getRaceDetails(id),
          odds: this.raceService.getRaceOdds(id).pipe(
            catchError(() => of({} as Record<number, number>))
          )
        });
      }),
      catchError((error: any) => {
        console.error('Error loading race details:', error);
        return of({ details: {} as RaceDetailDto, odds: {} as Record<number, number> });
      }),
      shareReplay(1)
    );

    // 2. Reactively fetch results based on the main data
    this.raceResult$ = this.raceDetailData$.pipe(
      switchMap((data) => {
        const status = data.details?.status?.toLowerCase();
        const season = data.details?.season;
        const id = data.details?.id;

        // If the race is finished, fetch the results. Otherwise, return null.
        if (
          (status === 'finished' || status === 'resultsprocessed') && 
          season !== undefined && 
          id !== undefined
        ) {
          return this.raceService.getStoredRaceResults(id).pipe(
            catchError((error: any) => {
              console.error('Error loading race results:', error);
              return of(null);
            })
          );
        }
        
        return of(null);
      })
    );
  }

  getStatusClass(status: string | undefined): string {
    if (!status) return 'unknown';
    return status.toLowerCase().replace(' ', '-');
  }

  getStatusText(status: string | undefined): string {
    if (!status) return 'Unknown';
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  getDriverIds(odds: Record<number, number>): string[] {
    if (!odds) return [];
    return Object.keys(odds);
  }

  getOddsValue(odds: Record<number, number>, driverId: string): number | undefined {
    if (!odds) return undefined;
    return odds[parseInt(driverId, 10)];
  }

  formatOdds(odds: number): string {
    return odds.toFixed(2);
  }
}