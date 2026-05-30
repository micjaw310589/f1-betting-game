import { Component, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core'; // <-- Dodany ChangeDetectorRef
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

  private readonly TEAM_COLORS: Record<string, string> = {
    'Red Bull Racing': '#367FA9',
    'Ferrari': '#E80020',
    'McLaren': '#FF8000',
    'Mercedes': '#27F4D2',
    'Aston Martin': '#229971',
    'Alpine': '#0093CC',
    'Haas F1 Team': '#B6BABD', // Zaktualizowane pod Twojego seeda "Haas F1 Team"
    'RB': '#6692FF',
    'Sauber': '#52E252',
    'Williams': '#64C4FF',
    'Audi': '#F50057'
  };

  // Wstrzykujemy cdr, dokładnie tak jak w profilu użytkownika
  constructor(
    private championshipService: ChampionshipService,
    private cdr: ChangeDetectorRef 
  ) {}

  ngOnInit(): void {
    this.loadStandings();
  }

loadStandings(): void {
    this.isLoading = true;
    this.hasError = false;

    this.subscription.add(
      this.championshipService.getCurrentStandings().subscribe({
        next: (data: any[]) => {
          console.log('Dane z API:', data);
          
          this.standings = (data || []).map(row => {
            // Bezpiecznie wyciągamy tablicę wyników wyścigów
            const results = row.raceResults || [];
            
            // Obliczamy wygrane: filtrujemy wyścigi, gdzie pozycja (positionInRace) była równa 1
            const calculatedWins = results.filter((r: any) => r.positionInRace === 1).length;
            
            // Obliczamy podia: filtrujemy wyścigi, gdzie pozycja była 1, 2 lub 3
            const calculatedPodiums = results.filter((r: any) => r.positionInRace >= 1 && r.positionInRace <= 3).length;

            return {
              id: row.driverId,
              driverId: row.driverId,
              driverName: row.driverName,
              teamName: row.teamName,
              season: row.season,
              position: row.position ?? 0,
              points: row.totalPoints ?? 0,
              wins: calculatedWins,       // <--- Wskakuje dynamicznie obliczona wartość!
              podiums: calculatedPodiums, // <--- Wskakuje dynamicznie obliczona wartość!
              lastUpdated: row.lastUpdated
            };
          });

          this.isLoading = false;
          this.cdr.detectChanges(); 
        },
        error: (err) => {
          console.error('Błąd w komponencie:', err);
          this.isLoading = false;
          this.hasError = true;
          this.errorMessage = 'Failed to load standings.';
          this.cdr.detectChanges();
        }
      })
    );
  }

  getTeamColor(teamName: string): string {
    return this.TEAM_COLORS[teamName] || '#FFFFFF';
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}