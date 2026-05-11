import { environment } from '../../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, catchError, map, of } from 'rxjs';
import {
  RaceSummaryDto,
  RaceDetailDto,
  RaceDto,
  PagedResult,
} from '../models/race.models';
import { DriverWithOdds } from '../bets/bet-placement/bet-placement.component';

@Injectable({
  providedIn: 'root',
})
export class RaceService {
  private readonly API_URL = `${environment.apiUrl}/Races`;

  constructor(private http: HttpClient) {}

  /**
   * Gets paginated race summaries with optional filtering.
   * @param page Page number (1-indexed)
   * @param pageSize Items per page
   * @param filterType 'all', 'upcoming', 'past', or a specific RaceStatus value
   */
  getRaceSummaries(page: number = 1, pageSize: number = 10, filterType: string = 'all'): Observable<PagedResult<RaceSummaryDto>> {
    // Map frontend filterType to backend status parameter
    // 'upcoming' maps to 'Scheduled', 'past' maps to 'Finished' (but backend filters exact status)
    const statusMap: Record<string, string> = {
      'all': '',
      'upcoming': 'Scheduled',
      'past': 'Finished',
      'scheduled': 'Scheduled',
      'in-progress': 'InProgress',
      'results-processed': 'ResultsProcessed'
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

  /**
   * Gets race details by race ID.
   * Note: The backend returns a limited RaceDetailDto with only basic fields:
   * Id, Name, Circuit, Country, RaceDate, Status, Season
   */
  getRaceDetails(raceId: number): Observable<RaceDetailDto> {
    return this.http.get<RaceDetailDto>(`${this.API_URL}/${raceId}`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Gets upcoming races with odds from the backend endpoint.
   * Uses the backend's GET /api/Races/upcoming/odds endpoint which returns RaceDto with Odds dictionary.
   */
  getUpcomingRacesWithOdds(): Observable<RaceDto[]> {
    return this.http.get<RaceDto[]>(`${this.API_URL}/upcoming/odds`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Gets odds for a specific race.
   * Note: The backend does NOT have a dedicated /{raceId}/odds endpoint.
   * Odds are available via GetUpcomingRacesWithOdds endpoint which returns RaceDto[].
   * This method now uses the correct backend approach by getting all upcoming races with odds
   * and filtering by the requested race ID.
   */
  getRaceOdds(raceId: number): Observable<Record<number, number>> {
    // Use the correct backend endpoint that actually exists
    return this.getUpcomingRacesWithOdds().pipe(
      map(races => {
        const race = races.find(r => r.id === raceId);
        if (race && race.odds) {
          // Convert Dictionary<int, decimal> to Record<number, number>
          const odds: Record<number, number> = {};
          for (const [driverId, oddValue] of Object.entries(race.odds)) {
            odds[parseInt(driverId, 10)] = Number(oddValue);
          }
          return odds;
        }
        return {};
      }),
      // If no odds available (e.g., race is not upcoming), return empty object instead of error
      catchError(() => of({}))
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

  // W race.service.ts dodaj:
getDriversWithOdds(raceId: number): Observable<DriverWithOdds[]> {
    return this.http.get<DriverWithOdds[]>(`${this.API_URL}/${raceId}/drivers-with-odds`);
  }
}
