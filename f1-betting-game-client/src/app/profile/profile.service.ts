import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { forkJoin, map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  BetHistoryResponseDto,
  UserProfileDto,
  BetHistoryDto,
  EnhancedUserStatisticsDto,
  UserBetAnalysisDto
} from './profile.models';

export interface ProfileAndBetsResponse {
  profile: UserProfileDto;
  betHistory: BetHistoryResponseDto;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private readonly API_URL = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfileDto> {
    return this.http.get<UserProfileDto>(`${this.API_URL}/profile`);
  }

  getBetHistory(page: number, pageSize: number): Observable<BetHistoryResponseDto> {
    const params = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));

    return this.http.get<BetHistoryResponseDto>(`${this.API_URL}/bets`, { params });
  }

  /**
   * Convenience method used by the component.
   * Fetches profile + bet history and returns them together.
   */
  getProfileAndBets(page: number, pageSize: number): Observable<ProfileAndBetsResponse> {
  return forkJoin({
    profile: this.getProfile(),
    betHistory: this.getBetHistory(page, pageSize)
  }).pipe(
    map(result => ({
      profile: result.profile,
      betHistory: result.betHistory
    }))
  );
}

  // Enhanced Statistics Methods
  getEnhancedStatistics(): Observable<EnhancedUserStatisticsDto> {
    return this.http.get<EnhancedUserStatisticsDto>(`${this.API_URL}/me/stats/enhanced`);
  }

  getBetHistoryWithFilters(
    userId: number,
    limit: number = 50,
    offset: number = 0,
    status?: string | null,
    driverId?: number | null
  ): Observable<BetHistoryDto[]> {
    let params = new HttpParams()
      .set('limit', String(limit))
      .set('offset', String(offset));

    if (status) {
      params = params.set('status', status);
    }
    if (driverId) {
      params = params.set('driverId', String(driverId));
    }

    return this.http.get<BetHistoryDto[]>(`${this.API_URL}/${userId}/bets/history`, { params });
  }

  getUserBetAnalysis(): Observable<UserBetAnalysisDto> {
    return this.http.get<UserBetAnalysisDto>(`${this.API_URL}/me/bets/analysis`);
  }

  getStatisticsByTimeRange(startDate: Date, endDate: Date): Observable<EnhancedUserStatisticsDto> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<EnhancedUserStatisticsDto>(`${this.API_URL}/me/stats/range`, { params });
  }
}
