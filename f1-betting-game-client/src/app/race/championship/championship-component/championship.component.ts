import { Component, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { RouterModule } from '@angular/router';
import { DriverChampionshipDto } from '../types/championship';
import { ChampionshipService } from '../championship.service';

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
    'Haas F1 Team': '#B6BABD', 
    'RB': '#6692FF',
    'Sauber': '#52E252',
    'Williams': '#64C4FF',
    'Audi': '#F50057',
    'Cadillac': '#F4A261' // <-- DODAJ TO
  };

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
        next: (data: DriverChampionshipDto[]) => {
          console.log('Pobrana klasyfikacja generalna:', data);
          
          this.standings = (data || []).map(row => ({
            driverId: row.driverId,
            driverName: row.driverName,
            driverCountry: row.driverCountry,
            teamName: row.teamName,
            season: row.season,
            position: row.position ?? 0,
            totalPoints: row.totalPoints ?? 0,
            lastUpdated: row.lastUpdated
          }));

          this.isLoading = false;
          this.cdr.detectChanges(); 
        },
        error: (err) => {
          console.error('Błąd pobierania danych na froncie:', err);
          this.isLoading = false;
          this.hasError = true;
          this.errorMessage = 'Failed to load championship standings.';
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