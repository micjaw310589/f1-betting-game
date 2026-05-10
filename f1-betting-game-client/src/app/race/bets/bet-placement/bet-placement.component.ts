import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subscription, catchError, of, switchMap } from 'rxjs';
import { RaceService } from '../../services/race.service';
import { BetService } from '../bet.service';
import { BetType, PlaceBetDto } from '../bet.models';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../auth/auth.service';

// Nowy interfejs pasujący do DTO z backendu
export interface DriverWithOdds {
  driverId: number;
  driverName: string;
  odds: number;
}

interface PendingBetView {
  betId: number;
  driverId: number;
  driverName?: string; // Dodane dla lepszego widoku
  amount: number;
  betType: BetType;
  createdAt: string;
}

@Component({
  selector: 'app-bet-placement',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './bet-placement.component.html',
  styleUrls: ['./bet-placement.component.css'],
})
export class BetPlacementComponent implements OnInit, OnDestroy {
  private subscriptions = new Subscription();

  raceId!: number;
  betTypeOptions: BetType[] = ['RaceWinner', 'PodiumFinish', 'Top10Finish', 'FastestLap'];

  // Zmienione: przechowujemy pełne obiekty kierowców
  driversList: DriverWithOdds[] = [];
  
  selectedBetType: BetType = 'RaceWinner';
  selectedDriverId?: number;

  amount = 0;
  isSubmitting = false;

  pendingBets: PendingBetView[] = [];
  betsLoading = false;
  betsError: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private raceService: RaceService,
    private betService: BetService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) {
      this.router.navigate(['/races']);
      return;
    }

    this.raceId = Number(idParam);

    // Ładowanie kierowców i ich kursów
    const sub = this.raceService.getDriversWithOdds(this.raceId).pipe(
      catchError(() => {
        window.alert('Failed to load drivers for this race.');
        return of([] as DriverWithOdds[]);
      }),
      switchMap(drivers => {
        this.driversList = drivers;
        // Domyślnie zaznacz pierwszego kierowcę z listy
        if (drivers.length > 0) {
          this.selectedDriverId = drivers[0].driverId;
        }
        return this.loadPendingBets();
      })
    ).subscribe();

    this.subscriptions.add(sub);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadPendingBets() {
    this.betsLoading = true;
    return this.betService.getMyBets().pipe(
      catchError(err => {
        this.betsError = 'Failed to load your bets.';
        return of([]);
      }),
      switchMap(allBets => {
        this.pendingBets = allBets
          .filter(b => b.raceId === this.raceId && b.status === 'Pending')
          .map(b => ({
            betId: b.id,
            driverId: b.driverId,
            driverName: b.driverName || `Driver ${b.driverId}`,
            amount: b.amount,
            betType: b.betType,
            createdAt: b.createdAt
          }));
        this.betsLoading = false;
        return of(null);
      })
    );
  }

  get canSubmit(): boolean {
    return this.amount > 0 && !!this.selectedDriverId && !this.isSubmitting;
  }

  // Pomocnicza metoda do pobrania kursu wybranego kierowcy (do wyświetlenia w UI)
  get selectedDriverOdds(): number {
    return this.driversList.find(d => d.driverId === this.selectedDriverId)?.odds || 0;
  }

  placeBet(): void {
    if (!this.canSubmit || this.selectedDriverId === undefined) return;

    const dto: PlaceBetDto = {
      raceId: this.raceId,
      driverId: this.selectedDriverId,
      amount: this.amount,
      betType: this.selectedBetType,
    };

    this.isSubmitting = true;
    const sub = this.betService.placeBet(dto).subscribe({
      next: () => {
        this.isSubmitting = false;
        window.alert('Bet placed successfully.');
        this.amount = 0;
        this.loadPendingBets().subscribe();
      },
      error: (err) => {
        this.isSubmitting = false;
        window.alert('Failed to place bet. ' + (err?.error?.message || err?.message || ''));
      }
    });

    this.subscriptions.add(sub);
  }

  cancelBet(betId: number): void {
    if (!window.confirm('Are you sure you want to cancel this bet?')) return;
    
    const sub = this.betService.cancelBet(betId).subscribe({
      next: () => {
        window.alert('Bet cancelled successfully.');
        this.loadPendingBets().subscribe();
      },
      error: () => window.alert('Failed to cancel bet.')
    });

    this.subscriptions.add(sub);
  }

  goBackToRaceDetail(): void {
    this.router.navigate(['/races', this.raceId]);
  }
}