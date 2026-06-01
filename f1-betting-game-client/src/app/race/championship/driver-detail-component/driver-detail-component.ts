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
  pointsProgression: { raceName: string; totalPoints: number }[] = [];
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
        this.isLoading = false; // Zapobiega forever loading, gdy brak ID
      }
    })
  );
}

loadDriverDetails(driverId: number): void {
    this.isLoading = true;
    this.subscription.add(
      this.championshipService.getDriverDetails(driverId).subscribe({
        next: (data: any) => { // Zmieniamy typ na any na potrzeby bezpiecznego mapowania
          console.log('DANE DOTARŁY DO KOMPONENTU:', data);
          if (data) {
            const results = data.raceResults || [];
            
            // Mapujemy właściwości z backendu na strukturę oczekiwaną przez szablon HTML
            const mappedResults = results.map((r: any) => ({
              raceId: r.raceId,
              raceName: r.raceName,
              position: r.positionInRace, // <--- Mapujemy positionInRace na position
              pointsEarned: r.pointsEarned,
              raceDate: r.raceDate
            }));

            // Ręcznie obliczamy zwycięstwa (P1) i podia (P1, P2, P3) z tablicy wyścigów
            const calculatedWins = results.filter((r: any) => r.positionInRace === 1).length;
            const calculatedPodiums = results.filter((r: any) => r.positionInRace >= 1 && r.positionInRace <= 3).length;

            this.driverData = {
              id: data.driverId,
              driverId: data.driverId,
              driverName: data.driverName,
              teamName: data.teamName,
              season: data.season,
              points: data.totalPoints ?? 0, // <--- Mapujemy totalPoints na points
              position: data.position ?? 0,
              wins: calculatedWins,          // <--- Przypisujemy obliczone wygrane
              podiums: calculatedPodiums,    // <--- Przypisujemy obliczone podia
              lastUpdated: data.lastUpdated,
              raceResults: mappedResults
            };

            this.calculatePointsProgression();
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

private calculatePointsProgression(): void {
  if (!this.driverData?.raceResults) return;

  let runningTotal = 0;
  
  // Bezpieczne sortowanie: najpierw sprawdzamy stabilne ID, a jeśli są daty – po datach
  const sortedRaces = [...this.driverData.raceResults].sort((a, b) => {
    if (a.raceDate && b.raceDate) {
      return new Date(a.raceDate).getTime() - new Date(b.raceDate).getTime();
    }
    // Fallback do ID, gdy brakuje dat w JSONie
    return a.raceId - b.raceId;
  });

  this.pointsProgression = sortedRaces.map(race => {
    runningTotal += race.pointsEarned;
    return {
      raceName: race.raceName.replace(' Grand Prix', ' GP'),
      totalPoints: runningTotal
    };
  });
}

  getTeamColor(teamName?: string): string {
    return teamName ? (this.TEAM_COLORS[teamName] || '#FFFFFF') : '#FFFFFF';
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  getSvgPoints(): string {
    if (!this.pointsProgression.length || !this.driverData?.points) return '';
    
    const maxPoints = this.driverData.points;
    const totalRaces = this.pointsProgression.length;

    return this.pointsProgression.map((point, i) => {
      // Obliczamy X w skali 0-100 na podstawie indeksu wyścigu
      const x = (i / (totalRaces - 1 || 1)) * 100;
      // Obliczamy Y w skali 0-40 (odwrócone, bo w SVG 0 jest na górze)
      const y = 40 - (point.totalPoints / maxPoints) * 40;
      return `${x},${y}`;
    }).join(' ');
  }
}