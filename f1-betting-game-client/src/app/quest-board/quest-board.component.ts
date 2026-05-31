import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';
import { QuestBoardService } from './quest-board.service';
import { QuestBoardDto, getCategoryConfig, getTypeBadge, getProgressPercentage, getProgressColor, getQuestState } from './quest-board.models';

@Component({
  selector: 'app-quest-board',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quest-board.component.html',
  styleUrl: './quest-board.component.css'
})
export class QuestBoardComponent implements OnInit {
  quests: QuestBoardDto[] = [];
  isLoading = true;
  error = '';
  isLoggedIn = false;

  constructor(
    private questBoardService: QuestBoardService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isLoggedIn();
    this.loadQuests();
  }

  loadQuests(): void {
    this.isLoading = true;
    this.error = '';

    this.questBoardService.getQuestBoard().subscribe({
      next: (quests) => {
        this.quests = quests;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load quests';
        this.isLoading = false;
      }
    });
  }

  getCategoryConfig(category: string): { value: string; label: string; emoji: string; color: string } {
    return getCategoryConfig(category);
  }

  getTypeBadge(isOneTime: boolean): string {
    return getTypeBadge(isOneTime);
  }

  getProgressPercentage(progress: number, target: number): number {
    return getProgressPercentage(progress, target);
  }

  getProgressColor(state: string): string {
    return getProgressColor(state as any);
  }

  getQuestState(progress: number | null, target: number, isCompleted: boolean | null | undefined, isClaimed: boolean | null | undefined): string {
    return getQuestState(progress, target, isCompleted ?? null, isClaimed ?? null);
  }

  // Group quests by category for display
  get groupedQuests(): { category: string; config: any; quests: QuestBoardDto[] }[] {
    const categories = ['Betting', 'Engagement', 'Achievement'];
    const result: { category: string; config: any; quests: QuestBoardDto[] }[] = [];

    for (const cat of categories) {
      const catQuests = this.quests.filter(q => q.category === cat);
      if (catQuests.length > 0) {
        result.push({
          category: cat,
          config: getCategoryConfig(cat),
          quests: catQuests
        });
      }
    }

    return result;
  }
}
