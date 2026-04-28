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
  // Assuming the base API path for races is available at /api/races
  private readonly API_URL = '/api/races';

  constructor(private http: HttpClient) {}

getRaceSummaries(page: number = 1, pageSize: number = 10, filterType: string = 'all'): Observable<PagedResult<RaceSummaryDto>> {
    // C# nie ma endpointu /summaries, ma główny GET /api/races
    return this.http.get<PagedResult<RaceSummaryDto>>(`${this.API_URL}?page=${page}&pageSize=${pageSize}`);
  }

  getRaceDetails(raceId: number): Observable<RaceDetailDto> {
    // C# ma [HttpGet("{raceId}")], więc ścieżka to /api/races/1
    return this.http.get<RaceDetailDto>(`${this.API_URL}/${raceId}`);
  }

  getRaceOdds(raceId: number): Observable<RaceDto> {
    // UWAGA: Twój C# obecnie nie ma endpointu "odds dla konkretnego ID".
    // Ma tylko zbiorczy: GET /api/races/upcoming/odds
    // Na potrzeby testu zmieńmy to na ten zbiorczy:
    return this.http.get<RaceDto[]>(`${this.API_URL}/upcoming/odds`).pipe(
      map(races => races.find(r => r.id === raceId)!) // Szukamy kursów dla konkretnego wyścigu w tablicy
    );
  }
}