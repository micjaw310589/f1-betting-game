import { Component, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { DriverChampionshipDto } from '../types/championship';
import { ChampionshipService } from '../championship.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-championship',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './championship.component.html',
  styleUrls: ['./championship.component.css']
})
export class ChampionshipComponent implements OnInit, OnDestroy {
  isLoading = true;
  hasError = false;
  errorMessage = '';
  
  standings: DriverChampionshipDto[] = [];
  private subscription = new Subscription();

  // Zamiast sztywnej listy, zaczynamy z pustą tablicą!
  availableSeasons: number[] = [];
  selectedSeason: number = 2026; // Sezon startowy

  private readonly TEAM_COLORS: Record<string, string> = {
    'Red Bull Racing': '#367FA9',
    'Ferrari': '#E80020',
    'McLaren': '#FF8000',
    'Mercedes': '#27F4D2',
    'Aston Martin': '#229971',
    'Alpine': '#0093CC',
    'Haas F1 Team': '#B6BABD', 
    'RB': '#6692FF',
    'Sauber': '#52E252',
    'Williams': '#64C4FF',
    'Audi': '#F50057'
  };

  constructor(
    private championshipService: ChampionshipService,
    private cdr: ChangeDetectorRef 
  ) {}

  ngOnInit(): void {
    // Na start ustawiamy rok z bieżącej daty (2026)
    this.selectedSeason = new Date().getFullYear();
    this.loadStandings();
  }

  loadStandings(): void {
    this.isLoading = true;
    this.hasError = false;

    this.subscription.add(
      this.championshipService.getSeasonStandings(this.selectedSeason).subscribe({
        next: (data: any[]) => {
          console.log(`Dane z API dla sezonu ${this.selectedSeason}:`, data);
          
          // 1. Mapowanie i obliczanie wyników (to co mieliśmy wcześniej)
          this.standings = (data || []).map(row => {
            const results = row.raceResults || [];
            const calculatedWins = results.filter((r: any) => r.positionInRace === 1).length;
            const calculatedPodiums = results.filter((r: any) => r.positionInRace >= 1 && r.positionInRace <= 3).length;

            return {
              id: row.driverId, 
              driverId: row.driverId,
              driverName: row.driverName,
              teamName: row.teamName,
              season: row.season,
              position: row.position ?? 0,
              points: row.totalPoints ?? 0, 
              wins: calculatedWins,       
              podiums: calculatedPodiums,    
              lastUpdated: row.lastUpdated
            };
          });

          // 2. DYNAMICZNE GENEROWANIE SEZONÓW
          // Jeśli z API przyszły jakieś dane, sprawdzamy czy lista przycisków zawiera już ten sezon.
          // Dodatkowo na stałe możemy dorzucić bazowe lata, żeby użytkownik miał co klikać.
          const defaultSeasons = [2026, 2025, 2024];
          const incomingSeasons = (data || []).map(row => row.season as number);
          
          // Łączymy domyślne sezony z tymi, które przyszły z API, wyciągamy UNIKALNE (Set) i sortujemy malejąco
          this.availableSeasons = Array.from(new Set([...defaultSeasons, ...incomingSeasons]))
            .filter(s => s > 0)
            .sort((a, b) => b - a);

          this.isLoading = false;
          this.cdr.detectChanges(); 
        },
        error: (err) => {
          console.error('Błąd w komponencie:', err);
          this.isLoading = false;
          this.hasError = true;
          this.errorMessage = `Failed to load standings for season ${this.selectedSeason}.`;
          this.cdr.detectChanges();
        }
      })
    );
  }

  changeSeason(season: number): void {
    if (this.selectedSeason === season) return;
    this.selectedSeason = season;
    this.loadStandings();
  }

  getTeamColor(teamName: string): string {
    return this.TEAM_COLORS[teamName] || '#FFFFFF';
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}