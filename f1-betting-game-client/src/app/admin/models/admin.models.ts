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
