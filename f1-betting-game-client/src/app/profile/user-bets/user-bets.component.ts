import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ProfileService } from '../profile.service';
import { BetHistoryDto } from '../profile.models';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DatePipe, CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-bets',
  templateUrl: './user-bets.component.html',
  styleUrls: ['./user-bets.component.css'],
  providers: [DatePipe],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class UserBetsComponent implements OnInit {
  bets: BetHistoryDto[] = [];
  filteredBets: BetHistoryDto[] = [];
  isLoading = true;
  error: string | null = null;
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  filterForm: FormGroup;
  statusOptions = ['Won', 'Lost', 'Pending', 'Canceled', 'Resolved'];
  driverOptions: { id: number, name: string }[] = [];
  math = Math;

  constructor(
    private profileService: ProfileService,
    private fb: FormBuilder,
    private datePipe: DatePipe,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      status: [''],
      driverId: [''],
      startDate: [''],
      endDate: ['']
    });
  }

  ngOnInit(): void {
    this.loadBets();
  }

  loadBets(): void {
    this.isLoading = true;
    this.error = null;

    const userId = 1;

    this.profileService.getBetHistoryWithFilters(userId, 100, 0).subscribe({
      next: (bets) => {
        this.bets = bets;
        this.filteredBets = [...bets];
        this.totalItems = bets.length;
        this.extractDriverOptions();
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading bet history:', err);
        this.error = 'Failed to load bet history. Please try again later.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  extractDriverOptions(): void {
    const uniqueDrivers = new Map<number, string>();
    this.bets.forEach(bet => {
      if (bet.driverName && !uniqueDrivers.has(bet.driverId)) {
        uniqueDrivers.set(bet.driverId, bet.driverName);
      }
    });

    this.driverOptions = Array.from(uniqueDrivers.entries()).map(([id, name]) => ({
      id,
      name: `${id} - ${name}`
    }));
  }

  applyFilters(): void {
    const { status, driverId, startDate, endDate } = this.filterForm.value;

    this.filteredBets = this.bets.filter(bet => {
      if (status && bet.status !== status) {
        return false;
      }

      if (driverId && bet.driverId !== Number(driverId)) {
        return false;
      }

      if (startDate || endDate) {
        const betDate = new Date(bet.createdAt);
        const start = startDate ? new Date(startDate) : null;
        const end = endDate ? new Date(endDate) : null;

        if (start && betDate < start) {
          return false;
        }

        if (end && betDate > end) {
          return false;
        }
      }

      return true;
    });

    this.totalItems = this.filteredBets.length;
    this.currentPage = 1;
  }

  resetFilters(): void {
    this.filterForm.reset();
    this.filteredBets = [...this.bets];
    this.totalItems = this.bets.length;
    this.currentPage = 1;
  }

  get paginatedBets(): BetHistoryDto[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredBets.slice(startIndex, startIndex + this.itemsPerPage);
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.itemsPerPage);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'won': return 'bg-green-100 text-green-800';
      case 'lost': return 'bg-red-100 text-red-800';
      case 'pending': return 'bg-yellow-100 text-yellow-800';
      case 'canceled': return 'bg-gray-100 text-gray-800';
      case 'resolved': return 'bg-blue-100 text-blue-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  formatDate(date: Date | string): string {
    return this.datePipe.transform(date, 'medium') || '';
  }

  exportToCSV(): void {
    if (!this.filteredBets.length) {
      return;
    }

    const headers = ['ID', 'Date', 'Race', 'Driver', 'Type', 'Amount', 'Status', 'Winnings'];
    const rows = this.filteredBets.map(bet => [
      bet.id,
      this.formatDate(bet.createdAt),
      bet.raceName || 'N/A',
      bet.driverName || `Driver ${bet.driverId}`,
      bet.betType,
      `$${bet.amount.toFixed(2)}`,
      bet.status,
      bet.winnings ? `$${bet.winnings.toFixed(2)}` : 'N/A'
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', `bet_history_${new Date().toISOString().slice(0, 10)}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  refreshData(): void {
    this.loadBets();
  }
}