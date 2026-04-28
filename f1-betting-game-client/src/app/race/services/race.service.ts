import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, catchError, tap, map } from 'rxjs';
import {
  RaceSummaryDto,
  RaceDetailDto,
  RaceDto,
  PagedResult,
} from '../models/race.models';

@Injectable({
  providedIn: 'root',
})
export class RaceService {
  private readonly API_URL = '/api/races';

  constructor(private http: HttpClient) {}

getRaceSummaries(page: number = 1, pageSize: number = 10, filterType: string = 'all'): Observable<PagedResult<RaceSummaryDto>> {
    return this.http.get<PagedResult<RaceSummaryDto>>(`${this.API_URL}?page=${page}&pageSize=${pageSize}`);
  }

  getRaceDetails(raceId: number): Observable<RaceDetailDto> {
    return this.http.get<RaceDetailDto>(`${this.API_URL}/${raceId}`);
  }

  getRaceOdds(raceId: number): Observable<RaceDto> {
    return this.http.get<RaceDto[]>(`${this.API_URL}/upcoming/odds`).pipe(
      map(races => races.find(r => r.id === raceId)!) // Szukamy kursów dla konkretnego wyścigu w tablicy
    );
  }
}