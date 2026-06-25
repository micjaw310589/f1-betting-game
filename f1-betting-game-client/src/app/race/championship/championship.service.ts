import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DriverChampionshipDto } from './types/championship';

@Injectable({
  providedIn: 'root'
})
export class ChampionshipService {
  // Gwarantujemy, że prefiks to /api/races, tak jak zdefiniowano w RacesController.cs
  private readonly apiUrl = `${environment.apiUrl}/races`; 

  constructor(private http: HttpClient) {}

  // Pobiera 20 rekordów z bazy danych dla bieżącego sezonu 2026
  getCurrentStandings(): Observable<DriverChampionshipDto[]> {
    return this.http.get<DriverChampionshipDto[]>(`${this.apiUrl}/championship/current`);
  }

  // Pobiera szczegóły kierowcy z bazy danych
  getDriverDetails(driverId: number): Observable<DriverChampionshipDto> {
    return this.http.get<DriverChampionshipDto>(`${this.apiUrl}/championship/driver/${driverId}`);
  }
}