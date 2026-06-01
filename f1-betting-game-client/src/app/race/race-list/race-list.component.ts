import { Component, OnInit, OnDestroy } from '@angular/core';
import { RaceService } from '../services/race.service';
import { Observable, Subscription, catchError, of, tap, shareReplay } from 'rxjs';
import { PagedResult, RaceSummaryDto } from '../models/race.models';
import { CommonModule } from '@angular/common'; // Dla async, ngIf, date
import { RouterModule } from '@angular/router'; // DLA [routerLink]
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-race-list',
  standalone: true,
  templateUrl: './race-list.component.html',
  imports: [CommonModule, RouterModule],
  styleUrls: ['./race-list.component.css'],
})
export class RaceListComponent implements OnInit, OnDestroy {
  isLoading = true;
  hasError = false;
  raceSummaries$!: Observable<PagedResult<RaceSummaryDto>>;
  private raceSubscription?: Subscription;

  constructor(public authService: AuthService, private raceService: RaceService) {}
  
  // Pagination controls
  page = 1;
  pageSize = 9;
  filterType: 'all' | 'upcoming' | 'past' |'live' = 'upcoming';


  ngOnInit(): void {
    this.loadRaceSummaries();
  }

  /**
   * Loads race summaries based on current pagination and filter criteria.
   * Uses shareReplay(1) so both the manual subscription (for loading state)
   * and the async pipe in the template share the same HTTP request.
   */
  loadRaceSummaries(): void {
    this.isLoading = true;
    this.hasError = false;
    
    // Cancel any previous subscription
    this.raceSubscription?.unsubscribe();
    
    this.raceSummaries$ = this.raceService.getRaceSummaries(this.page, this.pageSize, this.filterType).pipe(
      tap(() => {
        this.isLoading = false;
      }),
      catchError((error: any) => {
        console.error('Error loading races:', error);
        this.hasError = true;
        this.isLoading = false;
        return of({ items: [], page: this.page, pageSize: this.pageSize, totalItems: 0, totalPages: 0 });
      }),
      // Share the result so both the manual subscription and async pipe
      // use the same HTTP request instead of triggering two separate ones.
      shareReplay(1)
    );
    
    // Subscribe to trigger the request eagerly (the async pipe in the template
    // will receive the replayed result via shareReplay).
    this.raceSubscription = this.raceSummaries$.subscribe();
  }

  /**
   * Handles pagination changes.
   * @param pageNumber The page number to navigate to.
   */
  paginate(pageNumber: number): void {
    this.page = pageNumber;
    this.loadRaceSummaries();
  }

  /**
   * Filters the race list by category (Upcoming, Past, All).
   * @param filterType The type of filter to apply.
   */
  filterRaces(filterType: 'all' | 'upcoming' | 'past' | 'live'): void {
    this.filterType = filterType;
    this.page = 1; // Reset to the first page upon filtering
    this.loadRaceSummaries();
  }

  /**
   * Cleanup on component destroy.
   */
  ngOnDestroy(): void {
    this.raceSubscription?.unsubscribe();
  }

  /**
   * Helper function to map a RaceStatus enum to a CSS class.
   * @param status The race status.
   * @returns The corresponding CSS class name.
   */
getStatusClass(status: 'Scheduled' | 'InProgress' | 'Finished' | 'ResultsProcessed'): string {
  switch (status) {
      case 'Scheduled': return 'scheduled';
      case 'InProgress': return 'in-progress';
      case 'Finished': return 'finished';
      case 'ResultsProcessed': return 'results-processed'; // Poprawione: dopasowanie do klasy CSS
      default: return '';
  }
}
  /**
   * Helper function to return a readable status text.
   * @param status The race status.
   * @returns A descriptive string.
   */
  getStatusText(status: 'Scheduled' | 'InProgress' | 'Finished' | 'ResultsProcessed'): string {
    switch (status) {
        case 'Scheduled': return 'Scheduled';
        case 'InProgress': return 'In Progress';
        case 'Finished': return 'Finished';
        case 'ResultsProcessed': return 'Results Processed';
        default: return 'Unknown';
    }
  }
}
