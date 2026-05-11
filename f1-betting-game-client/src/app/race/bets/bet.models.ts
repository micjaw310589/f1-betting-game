/**
 * Bet frontend models (TypeScript) derived from backend DTOs:
 * - BetResponseDto
 * - PlaceBetDto
 * - BetsController return payloads
 */

export type BetType =
  | 'RaceWinner'
  | 'PodiumFinish'
  | 'Top10Finish'
  | 'FastestLap'
  | 'FastestPitStop'
  | 'DNFCount'
  | 'DriverVsDriver'
  | 'TeamVsTeam';

export interface BetResponseDto {
  id: number;
  userId: number;
  raceId: number;
  driverId: number;
  driverName?: string | null;

  amount: number;
  betType: BetType;
  status: 'Pending' | 'Won' | 'Lost' | 'Canceled' | 'Resolved';

  winnings?: number | null;
  createdAt: string; // API returns Date -> JSON string
  resolvedAt?: string | null;

  prediction?: number | null;
  predictionResult?: boolean | null;
}

export interface PlaceBetDto {
  raceId: number;
  driverId: number;
  amount: number;
  betType: BetType;

  // Not used for the TASK-03 simplified UI, but part of backend DTO
  placePosition?: number | null;
}

/**
 * Backend: BetsController.PlaceBet returns Ok(new { message = "...", userId })
 * (not the created BetResponseDto). We'll keep it defensive.
 */
export interface PlaceBetApiResponse {
  message?: string;
  userId?: number;

  // If backend changes to return bet DTO, we accept it too
  bet?: BetResponseDto;
  betResponse?: BetResponseDto;
}

/**
 * Backend: BetsController.CancelBet returns Ok(new { message = "...", betId = id })
 */
export interface CancelBetApiResponse {
  message?: string;
  betId?: number;
}
