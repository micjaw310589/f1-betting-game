// cd f1-betting-game-client
// ng test --include=src/app/profile/user-stats/user-stats.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserStatsComponent } from './user-stats.component';
import { ProfileService } from '../profile.service';
import { EnhancedUserStatisticsDto } from '../profile.models';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('UserStatsComponent', () => {
  let component: UserStatsComponent;
  let fixture: ComponentFixture<UserStatsComponent>;
  let profileServiceSpy: any;

  const mockStatistics: EnhancedUserStatisticsDto = {
    id: 1,
    username: 'testuser',
    email: 'test@example.com',
    points: 100,
    createdAt: new Date(),
    lastLoginAt: new Date(),
    totalBets: 100,
    winningBets: 60,
    losingBets: 30,
    pushBets: 10,
    winRate: 60,
    totalWinnings: 1000,
    returnOnInvestment: 15,
    currentWinStreak: 5,
    currentLoseStreak: 0,
    longestWinStreak: 10,
    favoriteDriverId: 1,
    favoriteDriverName: 'Lewis Hamilton',
    averageBetAmount: 50,
    largestWin: 500,
    largestLoss: 200,
    lastBetDate: new Date(),
    totalAmountBet: 5000,
    betsThisWeek: 5,
    betsThisMonth: 20
  };

  const spyOnComponentCdr = (comp: any) => {
    const cdrKey = Object.keys(comp).find(
      (key) => comp[key] && typeof comp[key].detectChanges === 'function'
    );
    if (!cdrKey) throw new Error('Nie znaleziono ChangeDetectorRef w komponencie!');
    return vi.spyOn(comp[cdrKey], 'detectChanges');
  };

  beforeEach(async () => {
    const profileServiceSpyObj = {
      getEnhancedStatistics: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [UserStatsComponent],
      providers: [
        { provide: ProfileService, useValue: profileServiceSpyObj }
      ]
    }).compileComponents();

    profileServiceSpy = TestBed.inject(ProfileService) as any;
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UserStatsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    profileServiceSpy.getEnhancedStatistics.mockReturnValue(of(mockStatistics));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should initialize with loading state', () => {
    expect(component.isLoading).toBeTruthy();
    expect(component.error).toBeNull();
    expect(component.statistics).toBeNull();
  });

  it('should load statistics on init', async () => {
    profileServiceSpy.getEnhancedStatistics.mockReturnValue(of(mockStatistics));
    const cdrSpy = spyOnComponentCdr(component);

    fixture.detectChanges();

    expect(component.isLoading).toBeFalsy();
    expect(component.error).toBeNull();
    expect(component.statistics).toEqual(mockStatistics);
    expect(cdrSpy).toHaveBeenCalled();
  });

  it('should handle error when loading statistics', async () => {
    const errorResponse = new Error('Failed to load statistics');
    profileServiceSpy.getEnhancedStatistics.mockReturnValue(throwError(() => errorResponse));

    fixture.detectChanges();

    expect(component.isLoading).toBeFalsy();
    expect(component.error).toBe('Failed to load statistics. Please try again later.');
    expect(component.statistics).toBeNull();
  });

  describe('getWinRateColor', () => {
    it('should return gray color when statistics are null', () => {
      component.statistics = null;
      expect(component.getWinRateColor()).toBe('text-gray-500');
    });

    it('should return green color when win rate > 60', () => {
      component.statistics = { ...mockStatistics, winRate: 65 };
      expect(component.getWinRateColor()).toBe('text-green-600');
    });

    it('should return blue color when win rate > 50', () => {
      component.statistics = { ...mockStatistics, winRate: 55 };
      expect(component.getWinRateColor()).toBe('text-blue-600');
    });

    it('should return yellow color when win rate > 40', () => {
      component.statistics = { ...mockStatistics, winRate: 45 };
      expect(component.getWinRateColor()).toBe('text-yellow-600');
    });

    it('should return red color when win rate <= 40', () => {
      component.statistics = { ...mockStatistics, winRate: 30 };
      expect(component.getWinRateColor()).toBe('text-red-600');
    });
  });

  describe('getROIColor', () => {
    it('should return gray color when statistics are null', () => {
      component.statistics = null;
      expect(component.getROIColor()).toBe('text-gray-500');
    });

    it('should return green color when ROI > 10', () => {
      component.statistics = { ...mockStatistics, returnOnInvestment: 15 };
      expect(component.getROIColor()).toBe('text-green-600');
    });

    it('should return blue color when ROI > 0', () => {
      component.statistics = { ...mockStatistics, returnOnInvestment: 5 };
      expect(component.getROIColor()).toBe('text-blue-600');
    });

    it('should return yellow color when ROI > -10', () => {
      component.statistics = { ...mockStatistics, returnOnInvestment: -5 };
      expect(component.getROIColor()).toBe('text-yellow-600');
    });

    it('should return red color when ROI <= -10', () => {
      component.statistics = { ...mockStatistics, returnOnInvestment: -15 };
      expect(component.getROIColor()).toBe('text-red-600');
    });
  });

  it('should refresh data', async () => {
    profileServiceSpy.getEnhancedStatistics.mockReturnValue(of(mockStatistics));
    fixture.detectChanges();

    const cdrSpy = spyOnComponentCdr(component);

    component.refreshData();

    expect(component.isLoading).toBeFalsy();
    expect(component.statistics).toEqual(mockStatistics);
    expect(cdrSpy).toHaveBeenCalled();
  });
});