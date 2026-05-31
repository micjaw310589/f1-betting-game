import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ProfileService } from '../profile.service';
import { UserBetAnalysisDto } from '../profile.models';
import { CommonModule, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-user-analytics',
  templateUrl: './user-analytics.component.html',
  styleUrls: ['./user-analytics.component.css'],
  standalone: true,
  imports: [CommonModule, DecimalPipe]
})
export class UserAnalyticsComponent implements OnInit {
  analysis: UserBetAnalysisDto | null = null;
  isLoading = true;
  error: string | null = null;

  constructor(
    private profileService: ProfileService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadAnalysis();
  }

  loadAnalysis(): void {
    this.isLoading = true;
    this.error = null;

    this.profileService.getUserBetAnalysis().subscribe({
      next: (analysis) => {
        this.analysis = analysis;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading analysis:', err);
        this.error = 'Failed to load analysis. Please try again later.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  refreshData(): void {
    this.loadAnalysis();
  }
}