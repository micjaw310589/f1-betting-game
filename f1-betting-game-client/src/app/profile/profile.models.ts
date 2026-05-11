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
