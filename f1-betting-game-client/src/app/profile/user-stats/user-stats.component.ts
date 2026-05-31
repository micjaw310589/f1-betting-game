import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ProfileService } from '../profile.service';
import { EnhancedUserStatisticsDto } from '../profile.models';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-stats',
  templateUrl: './user-stats.component.html',
  styleUrls: ['./user-stats.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class UserStatsComponent implements OnInit {
  statistics: EnhancedUserStatisticsDto | null = null;
  isLoading = true;
  error: string | null = null;

  constructor(private profileService: ProfileService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.isLoading = true;
    this.error = null;

    this.profileService.getEnhancedStatistics().subscribe({
      next: (stats) => {
        this.statistics = stats;
        this.isLoading = false;

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading statistics:', err);
        this.error = 'Failed to load statistics. Please try again later.';
        this.isLoading = false;
      }
    });
  }

  getWinRateColor(): string {
    if (!this.statistics) return 'text-gray-500';

    const winRate = this.statistics.winRate;
    if (winRate > 60) return 'text-green-600';
    if (winRate > 50) return 'text-blue-600';
    if (winRate > 40) return 'text-yellow-600';
    return 'text-red-600';
  }

  getROIColor(): string {
    if (!this.statistics) return 'text-gray-500';

    const roi = this.statistics.returnOnInvestment;
    if (roi > 10) return 'text-green-600';
    if (roi > 0) return 'text-blue-600';
    if (roi > -10) return 'text-yellow-600';
    return 'text-red-600';
  }

  refreshData(): void {
    this.loadStatistics();
  }
}