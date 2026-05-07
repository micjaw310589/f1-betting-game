import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { RaceService } from '../services/race.service';
import { Observable, of, forkJoin } from 'rxjs';
import { switchMap, map, catchError, shareReplay } from 'rxjs/operators';
import { RaceDetailDto } from '../models/race.models';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface RaceDetailData {
  details: RaceDetailDto;
  odds: Record<number, number>;
}

@Component({
  selector: 'app-race-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './race-detail.component.html',
  styleUrls: ['./race-detail.component.css'],
})
export class RaceDetailComponent implements OnInit {
  raceDetailData$!: Observable<RaceDetailData>;

  constructor(
    private activatedRoute: ActivatedRoute,
    private raceService: RaceService
  ) {}

  ngOnInit(): void {
    this.raceDetailData$ = this.activatedRoute.paramMap.pipe(
      map(params => params.get('id')!),
      switchMap((raceId: string) => {
        const id = Number(raceId);
        return forkJoin({
          details: this.raceService.getRaceDetails(id),
          odds: this.raceService.getRaceOdds(id).pipe(
            catchError(() => of({} as Record<number, number>))
          )
        });
      }),
      catchError(error => {
        console.error('Error loading race details:', error);
        // Return a minimal object so the template doesn't break
        return of({
          details: {} as RaceDetailDto,
          odds: {} as Record<number, number>
        });
      }),
      shareReplay(1)
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
