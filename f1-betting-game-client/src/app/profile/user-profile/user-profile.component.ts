import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { ProfileAndBetsResponse, ProfileService } from '../profile.service';
import { BetHistoryResponseDto, BetHistoryDto, UserProfileDto } from '../profile.models';
import { NavigationEnd, Router } from '@angular/router';
import { RaceService } from '../../race/services/race.service';
import { BetService } from '../../race/bets/bet.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css']
})
export class UserProfileComponent implements OnInit, OnDestroy {
  isLoading = true;
  hasError = false;
  errorMessage = '';

  profile: UserProfileDto | null = null;
  betHistory: BetHistoryResponseDto | null = null;

  // Pagination controls
  page = 1;
  pageSize = 10;

  private subscription = new Subscription();

constructor(
  private profileService: ProfileService,
  private router: Router,
  private cdr: ChangeDetectorRef,
  private raceService: RaceService,
  private betService: BetService
) {
  this.subscription.add( 
    this.router.events.subscribe((val) => {
      if (val instanceof NavigationEnd && (this.router.url === '/profile' || this.router.url === '/user-profile')) {
        this.load();
      }
    })
  );
}

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

load(): void {
  this.isLoading = true;
  this.cdr.detectChanges();

  this.profileService.getProfileAndBets(this.page, this.pageSize).subscribe({
    next: (result) => {
      this.profile = result.profile;
      this.betHistory = result.betHistory;

      // Mapujemy zakłady, żeby dociągnąć nazwy wyścigów, jeśli ich brakuje
      this.bets.forEach(bet => {
        if (!bet.raceName) {
          this.raceService.getRaceDetails(bet.raceId).subscribe(details => {
            bet.raceName = details.name;
            bet.raceDate = details.raceDate;
            this.cdr.detectChanges(); // Odśwież widok, gdy nazwa "dojedzie"
          });
        }
      });

      this.isLoading = false;
      this.cdr.detectChanges();
    },
    error: (err) => {
      this.hasError = true;
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  });
}
  paginate(pageNumber: number): void {
    this.page = pageNumber;
    this.load();
  }

  getBetStatusClass(status: string): string {
    // Backend sends BetStatus enum names serialized as strings (Won/Lost/...)
    // Keep it robust to unexpected casing.
    const normalized = (status || '').toLowerCase();

    if (normalized === 'won') return 'bet-won';
    if (normalized === 'lost') return 'bet-lost';
    if (normalized === 'canceled') return 'bet-canceled';
    if (normalized === 'pending') return 'bet-pending';

    return 'bet-unknown';
  }

  formatMoney(amount: number): string {
    return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(amount);
  }

  // Used by template for safer access
  get bets(): BetHistoryDto[] {
    return this.betHistory?.bets ?? [];
  }

  get totalPages(): number {
    return this.betHistory?.totalPages ?? 1;
  }

  get hasNextPage(): boolean {
    return this.betHistory?.hasNextPage ?? false;
  }

  get hasPreviousPage(): boolean {
    return this.betHistory?.hasPreviousPage ?? false;
  }

  // W klasie UserProfileComponent
formatBetType(type: string): string {
  if (!type) return '';
  // Dodaje spację przed wielkimi literami, np. RaceWinner -> Race Winner
  return type.replace(/([A-Z])/g, ' $1').trim();
}

cancelBet(betId: number): void {
  if (!window.confirm('Are you sure you want to cancel this bet?')) return;
  
  this.subscription.add(
    this.betService.cancelBet(betId).subscribe({
      next: () => {
        window.alert('Bet cancelled successfully.');
        this.load(); // Odświeżamy profil i historię, żeby punkty i lista wróciły do normy
      },
      error: () => window.alert('Failed to cancel bet.')
    })
  );
}
}
