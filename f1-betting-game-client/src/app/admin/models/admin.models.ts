/**
 * Interfaces for Admin User Management functionality.
 * These models define the structure of admin-specific data.
 */

/**
 * Corresponds to F1BettingApp.Application.DTOs.AdminUserDto
 */
export interface AdminUserDto {
    id: number;
    username: string;
    email: string;
    points: number;
    isActive: boolean;
    isAdmin: boolean;
    createdAt: Date;
    lastLogin: Date | null;
}

/**
 * DTO for adjusting a user's point balance (admin action).
 * Corresponds to F1BettingApp.Application.DTOs.AdjustUserPointsDto
 */
export interface AdjustUserPointsDto {
    points: number;
    reason?: string;
}

/**
 * Result of a point adjustment operation.
 * Corresponds to F1BettingApp.Application.DTOs.AdjustPointsResultDto
 */
export interface AdjustPointsResultDto {
    userId: number;
    username: string;
    newBalance: number;
    adjustedBy: number;
    reason: string | null;
    adjustedAt: Date;
}

/**
 * DTO for changing a user's account status.
 * Corresponds to F1BettingApp.Application.DTOs.ChangeUserStatusDto
 */
export interface ChangeUserStatusDto {
    isActive: boolean;
    reason?: string;
}

/**
 * Utility type for pagination results.
 */
export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
}

// ========================
// Bet Management Models
// ========================

/**
 * Bet type enum (mirrors backend BetType enum).
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

/**
 * Bet status enum (mirrors backend BetStatus enum).
 */
export type BetStatus = 'Pending' | 'Won' | 'Lost' | 'Canceled' | 'Resolved';

/**
 * Admin view of a bet, includes user and race context.
 * Corresponds to F1BettingApp.Application.DTOs.AdminBetResponseDto
 */
export interface AdminBetResponseDto {
    id: number;
    userId: number;
    username: string;
    raceId: number;
    raceName: string;
    driverId: number;
    driverName: string;
    amount: number;
    odds: number;
    betType: BetType;
    status: BetStatus;
    winnings: number | null;
    potentialWinnings: number | null;
    createdAt: string;
    resolvedAt: string | null;
}

/**
 * DTO for creating a bet as an admin.
 * Corresponds to F1BettingApp.Application.DTOs.CreateBetDto
 */
export interface CreateBetDto {
    userId: number;
    raceId: number;
    driverId: number;
    amount: number;
    betType: BetType;
}

/**
 * DTO for updating a bet as an admin.
 * Corresponds to F1BettingApp.Application.DTOs.UpdateBetDto
 */
export interface UpdateBetDto {
    driverId?: number;
    amount?: number;
    betType?: BetType;
    status?: BetStatus;
    winnings?: number;
}

/**
 * Available bet statuses for admin filtering.
 */
export const BET_STATUSES: { value: BetStatus; label: string }[] = [
    { value: 'Pending', label: 'Pending' },
    { value: 'Won', label: 'Won' },
    { value: 'Lost', label: 'Lost' },
    { value: 'Canceled', label: 'Canceled' },
    { value: 'Resolved', label: 'Resolved' },
] as const;

/**
 * Available bet types for admin forms.
 */
export const BET_TYPES: { value: BetType; label: string }[] = [
    { value: 'RaceWinner', label: 'Race Winner' },
    { value: 'PodiumFinish', label: 'Podium Finish' },
    { value: 'Top10Finish', label: 'Top 10 Finish' },
    { value: 'FastestLap', label: 'Fastest Lap' },
    { value: 'FastestPitStop', label: 'Fastest Pit Stop' },
    { value: 'DNFCount', label: 'DNF Count' },
    { value: 'DriverVsDriver', label: 'Driver vs Driver' },
    { value: 'TeamVsTeam', label: 'Team vs Team' },
] as const;

/**
 * Status badge styling mapping.
 */
export const BET_STATUS_CLASSES: Record<BetStatus, string> = {
    Pending: 'status-pending',
    Won: 'status-won',
    Lost: 'status-lost',
    Canceled: 'status-canceled',
    Resolved: 'status-resolved',
};

// ========================
// System Management Models
// ========================

/**
 * Result of a sync operation.
 * Corresponds to F1BettingApp.Application.DTOs.SyncResultDto
 */
export interface SyncResultDto {
    success: boolean;
    racesProcessed: number;
    racesCreated: number;
    racesUpdated: number;
    errorMessage: string | null;
    syncedAt: Date;
}

/**
 * Race DTO with override status (admin view).
 */
export interface AdminRaceDto {
    id: number;
    name: string;
    circuit: string;
    raceDate: Date;
    country: string;
    status: string;
    season: number;
    flag: string;
    odds: Record<number, number>;
    isManuallyOverridden: boolean;
}

// ========================
// Driver Models
// ========================

/**
 * Driver information for admin dropdowns.
 * Corresponds to F1BettingApp.Application.DTOs.DriverDto
 */
export interface DriverDto {
    id: number;
    name: string;
    abbreviation: string;
    teamId: number;
    teamName: string;
}

// ========================
// Race Metadata Override Models
// ========================

/**
 * DTO for updating race metadata (name, date, status, circuit, country).
 * Corresponds to F1BettingApp.Application.DTOs.UpdateRaceMetadataDto
 */
export interface UpdateRaceMetadataDto {
    name?: string;
    date?: string | null;
    circuit?: string;
    country?: string;
    status?: string;
}

/**
 * Available race statuses for admin override.
 */
export const RACE_STATUSES = [
    { value: 'Scheduled', label: 'Scheduled' },
    { value: 'InProgress', label: 'In Progress' },
    { value: 'Finished', label: 'Finished' },
    { value: 'ResultsProcessed', label: 'Results Processed' },
    { value: 'Cancelled', label: 'Cancelled' },
    { value: 'Postponed', label: 'Postponed' },
] as const;

// ========================
// Race Results Override Models
// ========================

/**
 * DTO for overriding race results manually (admin only).
 * Corresponds to F1BettingApp.Application.DTOs.OverrideRaceResultDto
 */
export interface OverrideRaceResultDto {
    positions: PositionEntryDto[];
    fastestLapDriverId?: number | null;
}

/**
 * Represents a finishing position with its driver ID.
 * Corresponds to F1BettingApp.Application.DTOs.PositionEntryDto
 */
export interface PositionEntryDto {
    position: number;
    driverId: number | null;
}

/**
 * Race result DTO with driver details (admin view).
 * Corresponds to F1BettingApp.Application.DTOs.RaceResultDto
 */
export interface RaceResultDto {
    raceId: number;
    raceName: string;
    circuit: string;
    country: string;
    raceDate: Date;
    winnerDriverId: number;
    winnerDriverName: string;
    winnerTeamId: number;
    winnerTeamName: string;
    fastestLapDriverId: number;
    fastestLapDriverName: string;
    winningMargin: number;
    polePositionDriverId: number;
    polePositionDriverName: string;
    safetyCar: number;
    virtualSafetyCar: number;
    redFlag: number;
    yellowFlag: number;
    blackFlag: number;
    blueFlag: number;
    blackAndWhiteFlag: number;
    chequeredFlag: number;
    raceDistance: number;
    raceDistanceUnit: number;
    laps: number;
    lapsCompleted: number;
    lapsToFinish: number;
    raceControlMessage: number;
    raceControlMessageText: string;
    timeAttack: string;
    timeAttackResult: string;
    timeAttackComment: string;
    timeAttackStatus: string;
    timeAttackLaps: string;
    positions: PositionItemDto[];
}

/**
 * A single finishing position entry with driver/team details.
 * Corresponds to F1BettingApp.Application.DTOs.PositionDto
 */
export interface PositionItemDto {
    position: number;
    driverId: number | null;
    driverName: string;
    teamId: number;
    teamName: string;
    points: number;
    fastestLap: Date | null;
    pitStopTime: Date | null;
}
