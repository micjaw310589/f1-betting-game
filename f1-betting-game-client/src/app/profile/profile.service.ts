import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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
    return new Observable<ProfileAndBetsResponse>((subscriber) => {
      let profile: UserProfileDto | null = null;
      let betHistory: BetHistoryResponseDto | null = null;

      const sub1 = this.getProfile().subscribe({
        next: (p) => {
          profile = p;
          if (betHistory) {
            subscriber.next({ profile, betHistory });
            subscriber.complete();
          }
        },
        error: (err) => subscriber.error(err)
      });

      const sub2 = this.getBetHistory(page, pageSize).subscribe({
        next: (b) => {
          betHistory = b;
          if (profile) {
            subscriber.next({ profile, betHistory });
            subscriber.complete();
          }
        },
        error: (err) => subscriber.error(err)
      });

      return () => {
        sub1.unsubscribe();
        sub2.unsubscribe();
      };
    });
  }
}
