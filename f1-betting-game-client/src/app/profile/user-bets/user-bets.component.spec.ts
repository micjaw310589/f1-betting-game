// cd f1-betting-game-client
// ng test --include=src/app/profile/user-bets/user-bets.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserBetsComponent } from './user-bets.component';
import { ProfileService } from '../profile.service';
import { BetHistoryDto } from '../profile.models';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { DatePipe } from '@angular/common';
import { vi } from 'vitest';

describe('UserBetsComponent', () => {
  let component: UserBetsComponent;
  let fixture: ComponentFixture<UserBetsComponent>;
  let profileServiceSpy: any;
  let datePipe: DatePipe;

  const mockBets: BetHistoryDto[] = [
    {
      id: 1,
      userId: '1',
      raceId: 1,
      driverId: 1,
      driverName: 'Lewis Hamilton',
      amount: 100,
      betType: 'RaceWinner',
      status: 'Won',
      winnings: 150,
      createdAt: new Date('2023-01-01'),
      resolvedAt: new Date('2023-01-02'),
      prediction: 1,
      predictionResult: true,
      raceName: 'Australian Grand Prix',
      raceDate: new Date('2023-01-01'),
      returnPercentage: 150
    },
    {
      id: 2,
      userId: '1',
      raceId: 2,
      driverId: 2,
      driverName: 'Max Verstappen',
      amount: 50,
      betType: 'PodiumFinish',
      status: 'Lost',
      winnings: 0,
      createdAt: new Date('2023-01-02'),
      resolvedAt: new Date('2023-01-03'),
      prediction: 2,
      predictionResult: false,
      raceName: 'Bahrain Grand Prix',
      raceDate: new Date('2023-01-02'),
      returnPercentage: 0
    },
    {
      id: 3,
      userId: '1',
      raceId: 3,
      driverId: 1,
      driverName: 'Lewis Hamilton',
      amount: 75,
      betType: 'RaceWinner',
      status: 'Pending',
      winnings: null,
      createdAt: new Date('2023-01-03'),
      resolvedAt: null,
      prediction: 1,
      predictionResult: null,
      raceName: 'Saudi Arabian Grand Prix',
      raceDate: new Date('2023-01-03'),
      returnPercentage: null
    }
  ];

  beforeEach(async () => {
    const profileServiceSpyObj = {
      getBetHistoryWithFilters: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [UserBetsComponent, ReactiveFormsModule],
      providers: [
        { provide: ProfileService, useValue: profileServiceSpyObj },
        DatePipe,
        FormBuilder
      ]
    }).compileComponents();

    profileServiceSpy = TestBed.inject(ProfileService);
    datePipe = TestBed.inject(DatePipe);
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(UserBetsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(of(mockBets));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should initialize with loading state', () => {
    expect(component.isLoading).toBe(true);
    expect(component.error).toBeNull();
    expect(component.bets).toEqual([]);
    expect(component.filteredBets).toEqual([]);
    expect(component.currentPage).toBe(1);
    expect(component.itemsPerPage).toBe(10);
  });

  it('should load bets on init', async () => {
    profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(of(mockBets));

    fixture.detectChanges();

    expect(component.isLoading).toBe(false);
    expect(component.error).toBeNull();
    expect(component.bets).toEqual(mockBets);
    expect(component.filteredBets).toEqual(mockBets);
    expect(component.totalItems).toBe(mockBets.length);
    expect(component.driverOptions.length).toBe(2);
  });

  it('should handle error when loading bets', async () => {
    const errorResponse = new Error('Failed to load bet history');
    profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(throwError(() => errorResponse));

    fixture.detectChanges();

    expect(component.isLoading).toBe(false);
    expect(component.error).toBe('Failed to load bet history. Please try again later.');
    expect(component.bets).toEqual([]);
    expect(component.filteredBets).toEqual([]);
  });

  describe('filtering', () => {
    beforeEach(() => {
      profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(of(mockBets));
      fixture.detectChanges();
    });

    it('should filter by status', () => {
      component.filterForm.get('status')?.setValue('Won');
      component.applyFilters();

      expect(component.filteredBets.length).toBe(1);
      expect(component.filteredBets[0].status).toBe('Won');
    });

    it('should filter by driver', () => {
      component.filterForm.get('driverId')?.setValue('1');
      component.applyFilters();

      expect(component.filteredBets.length).toBe(2);
      expect(component.filteredBets.every(bet => bet.driverId === 1)).toBe(true);
    });

    it('should filter by date range', () => {
      const startDate = new Date('2023-01-02');
      const endDate = new Date('2023-01-02');

      component.filterForm.get('startDate')?.setValue(startDate);
      component.filterForm.get('endDate')?.setValue(endDate);
      component.applyFilters();

      expect(component.filteredBets.length).toBe(1);
      expect(component.filteredBets[0].id).toBe(2);
    });

    it('should combine multiple filters', () => {
      component.filterForm.get('status')?.setValue('Won');
      component.filterForm.get('driverId')?.setValue('1');
      component.applyFilters();

      expect(component.filteredBets.length).toBe(1);
      expect(component.filteredBets[0].status).toBe('Won');
      expect(component.filteredBets[0].driverId).toBe(1);
    });
  });

  it('should reset filters', async () => {
    profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(of(mockBets));
    fixture.detectChanges();

    component.filterForm.get('status')?.setValue('Won');
    component.applyFilters();
    expect(component.filteredBets.length).toBe(1);

    component.resetFilters();
    expect(component.filteredBets.length).toBe(mockBets.length);
    expect(component.filterForm.value).toEqual({
      status: null,
      driverId: null,
      startDate: null,
      endDate: null
    });
  });

  describe('pagination', () => {
    beforeEach(() => {
      const manyBets = Array.from({ length: 25 }, (_, i) => ({
        ...mockBets[0],
        id: i + 1,
        createdAt: new Date(`2023-01-${String(i + 1).padStart(2, '0')}`)
      }));

      profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(of(manyBets));
      fixture.detectChanges();
    });

    it('should paginate bets', () => {
      expect(component.totalPages).toBe(3);
      expect(component.paginatedBets.length).toBe(10);

      component.changePage(2);
      expect(component.currentPage).toBe(2);
      expect(component.paginatedBets.length).toBe(10);

      component.changePage(3);
      expect(component.currentPage).toBe(3);
      expect(component.paginatedBets.length).toBe(5);
    });

    it('should not change page if page is out of range', () => {
      const initialPage = component.currentPage;
      component.changePage(0);
      expect(component.currentPage).toBe(initialPage);

      component.changePage(100);
      expect(component.currentPage).toBe(initialPage);
    });
  });

  it('should get correct status class', () => {
    expect(component.getStatusClass('Won')).toBe('bg-green-100 text-green-800');
    expect(component.getStatusClass('Lost')).toBe('bg-red-100 text-red-800');
    expect(component.getStatusClass('Pending')).toBe('bg-yellow-100 text-yellow-800');
    expect(component.getStatusClass('Canceled')).toBe('bg-gray-100 text-gray-800');
    expect(component.getStatusClass('Resolved')).toBe('bg-blue-100 text-blue-800');
    expect(component.getStatusClass('Unknown')).toBe('bg-gray-100 text-gray-800');
  });

  it('should format date correctly', () => {
    const testDate = new Date('2023-01-01T12:00:00');
    profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(of(mockBets));
    fixture.detectChanges();
    
    const formatted = component.formatDate(testDate);
    expect(formatted).toBe(datePipe.transform(testDate, 'medium'));
  });

  it('should export to CSV', () => {
    component.filteredBets = mockBets;

    const createElementSpy = vi.spyOn(document, 'createElement');
    const appendChildSpy = vi.spyOn(document.body, 'appendChild');
    const removeChildSpy = vi.spyOn(document.body, 'removeChild');

    component.exportToCSV();

    expect(createElementSpy).toHaveBeenCalledWith('a');
    expect(appendChildSpy).toHaveBeenCalled();
    expect(removeChildSpy).toHaveBeenCalled();
  });

  it('should refresh data', async () => {
    profileServiceSpy.getBetHistoryWithFilters.mockReturnValue(of(mockBets));
    fixture.detectChanges();

    component.refreshData();

    expect(component.bets).toEqual(mockBets);
    expect(component.filteredBets).toEqual(mockBets);
  });
});