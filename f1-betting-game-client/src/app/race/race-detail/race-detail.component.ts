import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { RaceService } from '../services/race.service';
import { Observable, combineLatest, of } from 'rxjs';
import { switchMap, catchError, map, tap } from 'rxjs/operators';
import { RaceDetailDto, RaceDto } from '../models/race.models';

import { CommonModule } from '@angular/common'; // Importuje async, ngIf, ngFor, date, number, ngClass
import { RouterModule } from '@angular/router'; // Jeśli używasz routerLink w tym komponencie

@Component({
  selector: 'app-race-detail',
  imports: [CommonModule, RouterModule],
  templateUrl: './race-detail.component.html',
  styleUrls: ['./race-detail.component.css'],
})
export class RaceDetailComponent implements OnInit {
  raceDetails$!: Observable<{ details: RaceDetailDto; odds: any }>;
  raceOdds$!: Observable<RaceDto>;

  constructor(
    private activatedRoute: ActivatedRoute,
    private raceService: RaceService
  ) {}

  ngOnInit(): void {
    // Get the race ID from the route parameters
    this.raceDetails$ = this.activatedRoute.paramMap.pipe(
      map(params => params.get('id')!),
      // Use switchMap to switch to the detail observable when raceId is available
      switchMap(raceId => this.raceService.getRaceDetails(Number(raceId)).pipe(
        // After getting details, immediately fetch the odds for that race
        switchMap(details => this.raceService.getRaceOdds(details.id).pipe(
          map(odds => ({ details, odds }))
        ))
      ))
    );
  }

  // Wewnątrz klasy RaceDetailComponent
getStatusClass(status: string | undefined): string {
  return status ? status.toLowerCase() : 'unknown';
}

getStatusText(status: string | undefined): string {
  return status || 'Unknown';
}

getDriverIds(odds: any): string[] {
  return odds ? Object.keys(odds) : [];
}
}