import { environment } from '../../../environments/environment';
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
  private readonly API_URL = `${environment.apiUrl}/races`;

  constructor(private http: HttpClient) {}

  getRaceSummaries(page: number = 1, pageSize: number = 10, filterType: string = 'all'): Observable<PagedResult<RaceSummaryDto>> {
    // Map frontend filterType to backend status parameter
    const statusMap: Record<string, string> = {
      'all': '',
      'upcoming': 'Scheduled',
      'past': 'Finished'
    };

    const status = statusMap[filterType] || '';
    const params: any = { page, pageSize };

    if (status) {
      params.status = status;
    }

    return this.http.get<PagedResult<RaceSummaryDto>>(this.API_URL, { params }).pipe(
      catchError(this.handleError)
    );
  }

  getRaceDetails(raceId: number): Observable<RaceDetailDto> {
    return this.http.get<RaceDetailDto>(`${this.API_URL}/${raceId}`).pipe(
      catchError(this.handleError)
    );
  }

  getRaceOdds(raceId: number): Observable<RaceDto> {
    return this.http.get<RaceDto[]>(`${this.API_URL}/upcoming/odds`).pipe(
      map(races => {
        const race = races.find(r => r.id === raceId);
        if (!race) {
          throw new Error(`Race with ID ${raceId} not found in odds data`);
        }
        return race;
      }),
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Unknown error occurred';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Server error: ${error.status}\nMessage: ${error.message}`;
    }
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
