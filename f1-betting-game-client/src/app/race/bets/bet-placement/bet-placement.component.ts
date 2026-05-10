import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subscription, catchError, of, switchMap } from 'rxjs';
import { RaceService } from '../../services/race.service';
import { BetService } from '../bet.service';
import { BetType, PlaceBetDto } from '../bet.models';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../auth/auth.service';

type DriverOddsMap = Record<number, number>;

interface PendingBetView {
  betId: number;
  driverId: number;
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

  odds: DriverOddsMap = {};
  driverIds: number[] = [];

  selectedBetType: BetType = 'RaceWinner';
  selectedDriverId?: number;

  amount = 0;
  isSubmitting = false;

  pendingBets: PendingBetView[] = [];
  betsLoading = false;
  betsError: string | null = null;

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private raceService: RaceService,
    private betService: BetService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const sub = this.activatedRoute.paramMap
      .pipe(
        switchMap(params => {
          const id = Number(params.get('id'));
          this.raceId = id;

          // Load odds (drivers list) for this race
          return this.raceService.getRaceOdds(id).pipe(
            catchError(() => of({} as DriverOddsMap)),
            switchMap(odds => {
              this.odds = odds;
              this.driverIds = Object.keys(odds).map(k => Number(k)).sort((a, b) => a - b);

              // Load current user's bets for cancellation UI
              return this.loadPendingBets();
            })
          );
        })
      )
      .subscribe();

    this.subscriptions.add(sub);
  }

  private loadPendingBets() {
    this.betsLoading = true;
    this.betsError = null;

    return this.betService.getMyBets().pipe(
      switchMap(bets => {
        this.pendingBets = bets
          .filter(b => b.status === 'Pending' && b.raceId === this.raceId)
          .map(b => ({
            betId: b.id,
            driverId: b.driverId,
            amount: b.amount,
            betType: b.betType,
            createdAt: b.createdAt,
          }));

        this.betsLoading = false;
        if (this.selectedDriverId === undefined && this.driverIds.length > 0) {
          this.selectedDriverId = this.driverIds[0];
        }

        return of(null);
      }),
      catchError(err => {
        this.betsLoading = false;
        this.betsError = 'Failed to load your pending bets.';
        return of(null);
      })
    );
  }

  get canSubmit(): boolean {
    const amountOk = this.amount > 0;
    const driverOk = this.selectedDriverId !== undefined;
    return amountOk && driverOk && !this.isSubmitting;
  }

  get canCancel(): boolean {
    return this.pendingBets.length > 0;
  }

  get selectedDriverOdds(): number | undefined {
    if (this.selectedDriverId === undefined) return undefined;
    return this.odds[this.selectedDriverId];
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

    const sub = this.betService.placeBet(dto).pipe(
      catchError(err => {
        this.isSubmitting = false;
        window.alert('Failed to place bet. ' + (err?.message ?? ''));
        return of(null);
      })
    ).subscribe(result => {
      this.isSubmitting = false;

      // Backend currently returns { message, userId } (defensive)
      if (result) {
        window.alert('Bet placed successfully.');
        this.amount = 0;
        this.selectedBetType = 'RaceWinner';
        this.loadPendingBets().subscribe();
      } else {
        // error already alerted
      }
    });

    this.subscriptions.add(sub);
  }

  cancelBet(betId: number): void {
    const sub = this.betService.cancelBet(betId).pipe(
      catchError(err => {
        window.alert('Failed to cancel bet.');
        return of(null);
      })
    ).subscribe(res => {
      if (res) {
        window.alert('Bet cancelled successfully.');
        this.loadPendingBets().subscribe();
      }
    });

    this.subscriptions.add(sub);
  }

  goBackToRaceDetail(): void {
    this.router.navigate(['/races', this.raceId]);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
