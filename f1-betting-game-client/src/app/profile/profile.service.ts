import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { forkJoin, map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BetHistoryResponseDto, UserProfileDto } from './profile.models';

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
}
