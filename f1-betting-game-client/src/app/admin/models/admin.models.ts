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
 * DTO for overriding race results (admin).
 * Corresponds to F1BettingApp.Application.DTOs.OverrideRaceResultDto
 */
export interface OverrideRaceResultDto {
    positions: PositionEntryDto[];
    fastestLapDriverId: number | null;
}

/**
 * A finishing position with its driver ID.
 * Corresponds to F1BettingApp.Application.DTOs.PositionEntryDto
 */
export interface PositionEntryDto {
    position: number;
    driverId: number;
}

/**
 * Race result DTO with driver details (admin view).
 * Corresponds to F1BettingApp.Application.DTOs.RaceResultDto
 */
export interface AdminRaceResultDto {
    raceId: number;
    raceName: string;
    circuit: string;
    country: string;
    raceDate: Date;
    winnerDriverId: number;
    winnerDriverName: string;
    winnerTeamId: number;
    winnerTeamName: string;
    winningMargin: number;
    fastestLapDriverId: number;
    fastestLapDriverName: string;
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
