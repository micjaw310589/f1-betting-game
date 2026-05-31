// cd f1-betting-game-client
// ng test --include=src/app/profile/user-analytics/user-analytics.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserAnalyticsComponent } from './user-analytics.component';
import { ProfileService } from '../profile.service';
import { UserBetAnalysisDto } from '../profile.models';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('UserAnalyticsComponent', () => {
  let component: UserAnalyticsComponent;
  let fixture: ComponentFixture<UserAnalyticsComponent>;
  let profileServiceSpy: any;

  const mockAnalysis: UserBetAnalysisDto = {
    userId: 1,
    betTypeAnalysis: {
      'RaceWinner': {
        totalBets: 10,
        winningBets: 6,
        winRate: 60,
        totalAmount: 500,
        totalWinnings: 300,
        roi: 15
      },
      'PodiumFinish': {
        totalBets: 5,
        winningBets: 3,
        winRate: 60,
        totalAmount: 250,
        totalWinnings: 150,
        roi: 10
      }
    },
    driverAnalysis: {
      1: {
        driverName: 'Lewis Hamilton',
        totalBets: 5,
        winningBets: 3,
        winRate: 60,
        totalWinnings: 200
      },
      2: {
        driverName: 'Max Verstappen',
        totalBets: 3,
        winningBets: 2,
        winRate: 66.67,
        totalWinnings: 150
      }
    },
    teamAnalysis: {
      1: {
        teamName: 'Mercedes',
        totalBets: 5,
        winningBets: 3,
        winRate: 60,
        totalWinnings: 200
      },
      2: {
        teamName: 'Red Bull',
        totalBets: 3,
        winningBets: 2,
        winRate: 66.67,
        totalWinnings: 150
      }
    },
    monthlyAnalysis: [
      {
        year: 2023,
        month: 1,
        totalBets: 5,
        winningBets: 3,
        totalWinnings: 150,
        winRate: 60
      },
      {
        year: 2023,
        month: 2,
        totalBets: 8,
        winningBets: 5,
        totalWinnings: 250,
        winRate: 62.5
      }
    ],
    timeOfDayAnalysis: {
      morningBets: 2,
      afternoonBets: 5,
      eveningBets: 3,
      nightBets: 0,
      morningWinRate: 50,
      afternoonWinRate: 60,
      eveningWinRate: 66.67,
      nightWinRate: 0
    }
  };

  beforeEach(async () => {
    const profileServiceSpyObj = {
      getUserBetAnalysis: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [UserAnalyticsComponent],
      providers: [
        { provide: ProfileService, useValue: profileServiceSpyObj }
      ]
    }).compileComponents();

    profileServiceSpy = TestBed.inject(ProfileService);
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UserAnalyticsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    profileServiceSpy.getUserBetAnalysis.mockReturnValue(of(mockAnalysis));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should initialize with loading state', () => {
    expect(component.isLoading).toBe(true);
    expect(component.error).toBeNull();
    expect(component.analysis).toBeNull();
  });

  it('should load analysis on init', async () => {
    profileServiceSpy.getUserBetAnalysis.mockReturnValue(of(mockAnalysis));

    fixture.detectChanges();

    expect(component.isLoading).toBe(false);
    expect(component.error).toBeNull();
    expect(component.analysis).toEqual(mockAnalysis);
  });

  it('should handle error when loading analysis', async () => {
    const errorResponse = new Error('Failed to load analysis');
    profileServiceSpy.getUserBetAnalysis.mockReturnValue(throwError(() => errorResponse));

    fixture.detectChanges();

    expect(component.isLoading).toBe(false);
    expect(component.error).toBe('Failed to load analysis. Please try again later.');
    expect(component.analysis).toBeNull();
  });

  it('should refresh data', async () => {
    profileServiceSpy.getUserBetAnalysis.mockReturnValue(of(mockAnalysis));
    fixture.detectChanges();

    component.refreshData();

    expect(component.isLoading).toBe(false);
    expect(component.analysis).toEqual(mockAnalysis);
  });
});