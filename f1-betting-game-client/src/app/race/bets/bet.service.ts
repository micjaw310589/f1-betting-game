import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BetResponseDto, CancelBetApiResponse, PlaceBetDto, PlaceBetApiResponse } from './bet.models';

@Injectable({
  providedIn: 'root',
})
export class BetService {
  private readonly API_URL = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  /**
   * Place a new bet on a scheduled race.
   * Zmieniono: `${this.API_URL}/api/bets/place` -> `${this.API_URL}/bets/place`
   */
  placeBet(dto: PlaceBetDto): Observable<BetResponseDto | PlaceBetApiResponse> {
    return this.http.post<BetResponseDto | PlaceBetApiResponse>(`${this.API_URL}/bets/place`, dto);
  }

  /**
   * Cancel an existing bet.
   * Zmieniono: `${this.API_URL}/api/bets/${betId}/cancel` -> `${this.API_URL}/bets/${betId}/cancel`
   */
  cancelBet(betId: number): Observable<CancelBetApiResponse> {
    return this.http.post<CancelBetApiResponse>(`${this.API_URL}/bets/${betId}/cancel`, {});
  }

  /**
   * Get all bets for the current user.
   * Zmieniono: `${this.API_URL}/api/bets` -> `${this.API_URL}/bets`
   */
  getMyBets(): Observable<BetResponseDto[]> {
    return this.http.get<BetResponseDto[]>(`${this.API_URL}/bets`);
  }
}