import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DriverChampionshipDto } from './types/championship';

@Injectable({
  providedIn: 'root'
})
export class ChampionshipService {
  private readonly API_URL = `${environment.apiUrl}/races/championship`;

  constructor(private http: HttpClient) {}

  /**
   * Pobiera aktualną klasyfikację generalną kierowców
   */
  getCurrentStandings(): Observable<DriverChampionshipDto[]> {
    return this.http.get<DriverChampionshipDto[]>(`${this.API_URL}/current`);
  }

  /**
   * Pobiera szczegółową historię wyścigów danego kierowcy
   */
  getDriverDetails(driverId: number): Observable<DriverChampionshipDto> {
    return this.http.get<DriverChampionshipDto>(`${this.API_URL}/driver/${driverId}`);
  }
}