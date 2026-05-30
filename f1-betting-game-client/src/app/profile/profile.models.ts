/**
 * Frontend models derived from backend DTOs for Task-05:
 * - UserProfileDto
 * - BetHistoryDto
 * - BetHistoryResponseDto
 */

export interface UserProfileDto {
  id: number;
  username: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  bio?: string | null;
  points: number;
  createdAt: Date;
  lastLoginAt: Date;
}

export interface BetHistoryDto {
  id: number;
  userId: string;
  raceId: number;
  driverId: number;
  driverName?: string | null;
  amount: number;
  betType: string;
  status: string; // e.g. "Won" | "Lost" | "Pending" | "Canceled"
  winnings?: number | null;
  createdAt: Date;
  resolvedAt?: Date | null;
  prediction?: number | null;
  predictionResult?: boolean | null;
  raceName?: string | null;
  raceDate?: Date | null;
  returnPercentage?: number | null;
}

export interface BetHistoryResponseDto {
  bets: BetHistoryDto[];

  totalCount: number;
  pageNumber: number;
  pageSize: number;

  // Derived from backend DTO semantics; depending on API mapping these may or may not exist.
  // We'll treat them as optional on the frontend to be resilient.
  totalPages?: number;
  hasNextPage?: boolean;
  hasPreviousPage?: boolean;
}

// Enhanced Statistics Models
export interface EnhancedUserStatisticsDto extends UserProfileDto {
  // Basic statistics
  totalBets: number;
  winningBets: number;
  losingBets: number;
  pushBets: number;
  winRate: number;
  totalWinnings: number;

  // Advanced metrics
  returnOnInvestment: number; // ROI percentage
  currentWinStreak: number;
  currentLoseStreak: number;
  longestWinStreak: number;
  favoriteDriverId: number;
  favoriteDriverName: string;
  averageBetAmount: number;
  largestWin: number;
  largestLoss: number;
  lastBetDate?: Date | null;
  totalAmountBet: number;
  betsThisWeek: number;
  betsThisMonth: number;
}

export interface UserBetAnalysisDto {
  userId: number;
  betTypeAnalysis: Record<string, BetTypeAnalysisDto>;
  driverAnalysis: Record<number, DriverAnalysisDto>;
  teamAnalysis: Record<number, TeamAnalysisDto>;
  monthlyAnalysis: MonthlyAnalysisDto[];
  timeOfDayAnalysis: TimeOfDayAnalysisDto;
}

export interface BetTypeAnalysisDto {
  totalBets: number;
  winningBets: number;
  winRate: number;
  totalAmount: number;
  totalWinnings: number;
  roi: number;
}

export interface DriverAnalysisDto {
  driverName: string;
  totalBets: number;
  winningBets: number;
  winRate: number;
  totalWinnings: number;
}

export interface TeamAnalysisDto {
  teamName: string;
  totalBets: number;
  winningBets: number;
  winRate: number;
  totalWinnings: number;
}

export interface MonthlyAnalysisDto {
  year: number;
  month: number;
  totalBets: number;
  winningBets: number;
  totalWinnings: number;
  winRate: number;
}

export interface TimeOfDayAnalysisDto {
  morningBets: number;
  afternoonBets: number;
  eveningBets: number;
  nightBets: number;
  morningWinRate: number;
  afternoonWinRate: number;
  eveningWinRate: number;
  nightWinRate: number;
}
