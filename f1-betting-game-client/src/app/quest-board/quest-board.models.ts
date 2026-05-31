/**
 * Interfaces for Quest Board functionality.
 * These models define the structure of quest-related data.
 */

/**
 * DTO for the quest board endpoint (user-facing).
 * Corresponds to F1BettingApp.Application.DTOs.QuestBoardDto
 */
export interface QuestBoardDto {
  questId: string;
  name: string;
  description: string;
  category: string;
  isOneTime: boolean;
  target: number;
  pointsReward: number;
  isActive: boolean;
  order: number;
  progress?: number | null;
  isCompleted?: boolean | null;
  isClaimed?: boolean | null;
}

/**
 * Quest category configuration.
 */
export const QUEST_CATEGORIES: { value: string; label: string; emoji: string; color: string }[] = [
  { value: 'Betting', label: 'Betting', emoji: '🏎️', color: '#00d4ff' },
  { value: 'Engagement', label: 'Engagement', emoji: '⚡', color: '#4caf50' },
  { value: 'Achievement', label: 'Achievement', emoji: '🏆', color: '#ffc107' },
];

/**
 * Get category configuration by value.
 */
export function getCategoryConfig(category: string): { value: string; label: string; emoji: string; color: string } {
  return QUEST_CATEGORIES.find(c => c.value === category) || QUEST_CATEGORIES[0];
}

/**
 * Get the quest type badge text.
 */
export function getTypeBadge(isOneTime: boolean): string {
  return isOneTime ? 'One-time' : 'Weekly';
}

/**
 * Get the progress percentage for a quest.
 */
export function getProgressPercentage(progress: number, target: number): number {
  if (target <= 0) return 0;
  return Math.min(100, Math.round((progress / target) * 100));
}

/**
 * Get the progress bar color based on quest state.
 */
export function getProgressColor(state: 'not-started' | 'in-progress' | 'completed' | 'claimed'): string {
  switch (state) {
    case 'not-started': return '#555';
    case 'in-progress': return '#00d4ff';
    case 'completed': return '#4caf50';
    case 'claimed': return '#4caf50';
    default: return '#555';
  }
}

/**
 * Determine the current state of a quest based on progress and claim status.
 */
export function getQuestState(progress: number | null, target: number, isCompleted: boolean | null, isClaimed: boolean | null): 'not-started' | 'in-progress' | 'completed' | 'claimed' {
  if (isClaimed) return 'claimed';
  if (isCompleted) return 'completed';
  if (progress !== null && progress > 0) return 'in-progress';
  return 'not-started';
}
