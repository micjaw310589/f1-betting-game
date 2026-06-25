import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { DriverChampionshipDto } from '../types/championship';
import { Subscription } from 'rxjs';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ChampionshipService } from '../championship.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-driver-detail-component',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './driver-detail-component.html',
  styleUrl: './driver-detail-component.css',
})
export class DriverDetailComponent implements OnInit, OnDestroy {
  isLoading = true;
  hasError = false;
  errorMessage = '';
  
  driverData?: DriverChampionshipDto;
  private subscription = new Subscription();

  private readonly TEAM_COLORS: Record<string, string> = {
    'Red Bull Racing': '#367FA9', 'Ferrari': '#E80020', 'McLaren': '#FF8000',
    'Mercedes': '#27F4D2', 'Aston Martin': '#229971', 'Alpine': '#0093CC',
    'Haas F1 Team': '#B6BABD', 'RB': '#6692FF', 'Sauber': '#52E252',
    'Williams': '#64C4FF', 'Audi': '#F50057', 'Cadillac': '#F4A261'
  };

  constructor(
    private route: ActivatedRoute,
    private championshipService: ChampionshipService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.subscription.add(
      this.route.paramMap.subscribe(params => {
        const driverIdParam = params.get('id');
        if (driverIdParam) {
          this.loadDriverDetails(Number(driverIdParam));
        } else {
          this.isLoading = false;
        }
      })
    );
  }

  loadDriverDetails(driverId: number): void {
    this.isLoading = true;
    this.subscription.add(
      this.championshipService.getDriverDetails(driverId).subscribe({
        next: (data: any) => {
          console.log('Dane kierowcy dotarły do komponentu:', data);
          if (data) {
            // Przypisujemy uproszczone dane współgrające z typem DriverChampionshipDto
            this.driverData = {
              driverId: data.driverId,
              driverName: data.driverName,
              driverCountry: data.driverCountry,
              teamName: data.teamName,
              season: data.season,
              totalPoints: data.totalPoints ?? 0, // Zmiana z .points na .totalPoints
              position: data.position ?? 0,
              lastUpdated: data.lastUpdated
            };
          }
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error(err);
          this.hasError = true;
          this.errorMessage = 'Failed to load driver performance dataset.';
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      })
    );
  }

  getTeamColor(teamName?: string): string {
    return teamName ? (this.TEAM_COLORS[teamName] || '#FFFFFF') : '#FFFFFF';
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}