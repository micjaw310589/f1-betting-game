import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { ProfileAndBetsResponse, ProfileService } from '../profile.service';
import { BetHistoryResponseDto, BetHistoryDto, DailyStreakResponse, PointHistoryResponseDto, QuestResponse, UserProfileDto } from '../profile.models';
import { NavigationEnd, Router } from '@angular/router';
import { RaceService } from '../../race/services/race.service';
import { BetService } from '../../race/bets/bet.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css']
})
export class UserProfileComponent implements OnInit, OnDestroy {
  isLoading = true;
  hasError = false;
  errorMessage = '';

  profile: UserProfileDto | null = null;
  betHistory: BetHistoryResponseDto | null = null;

  // New state properties for streak, quests, and point history
  dailyStreak: DailyStreakResponse | null = null;
  quests: QuestResponse[] = [];
  pointHistory: PointHistoryResponseDto | null = null;
  pointHistoryPage = 1;
  pointHistoryPageSize = 10;

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

      // Load streak, quests, and point history
      this.loadStreakAndQuests();
      this.loadPointHistory();

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

loadStreakAndQuests(): void {
  this.profileService.getDailyStreak().subscribe({
    next: (streak) => {
      this.dailyStreak = streak;
      this.cdr.detectChanges();
    },
    error: () => {
      // Streak data is optional - ignore errors
    }
  });

  this.profileService.getQuests().subscribe({
    next: (quests) => {
      this.quests = quests;
      this.cdr.detectChanges();
    },
    error: () => {
      // Quests data is optional - ignore errors
    }
  });
}

loadPointHistory(): void {
  this.profileService.getPointHistory(this.pointHistoryPage, this.pointHistoryPageSize).subscribe({
    next: (history) => {
      this.pointHistory = history;
      this.cdr.detectChanges();
    },
    error: () => {
      this.pointHistory = { items: [], totalCount: 0, pageNumber: this.pointHistoryPage, pageSize: this.pointHistoryPageSize };
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

// Helper methods for the template

getStreakMultiplier(streak: number): string {
  if (streak >= 7) return '×2.5';
  if (streak >= 5) return '×2';
  if (streak >= 3) return '×1.5';
  return '×1';
}

getCategoryColor(category: string): string {
  switch (category.toLowerCase()) {
    case 'betting': return '#ff6b6b';
    case 'engagement': return '#4ecdc4';
    case 'achievement': return '#ffd93d';
    default: return '#888';
  }
}

getPointsClass(points: number): string {
  return points > 0 ? 'points-positive' : points < 0 ? 'points-negative' : '';
}

formatCategory(category: string): string {
  // e.g. "DailyLogin" -> "Daily Login", "BetWin" -> "Bet Win"
  return category.replace(/([A-Z])/g, ' $1').replace(/^./, s => s.toUpperCase()).trim();
}

get hasMorePointHistory(): boolean {
  return this.pointHistory?.hasNextPage ?? false;
}

get hasPreviousPointHistory(): boolean {
  return this.pointHistory?.hasPreviousPage ?? false;
}

loadMorePointHistory(page: number): void {
  this.pointHistoryPage = page;
  this.loadPointHistory();
}
}
