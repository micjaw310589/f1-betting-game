/**
 * Interfaces derived from C# DTOs for Race functionality.
 * These models define the structure of race data across different views and list types.
 */

/**
 * Corresponds to F1BettingApp.Application.DTOs.RaceSummaryDto
 * Lightweight DTO for race summary information in lists and search results.
 */
export interface RaceSummaryDto {
    id: number;
    name: string;
    circuit: string;
    country: string;
    raceDate: Date;
    status: 'Scheduled' | 'InProgress' | 'Finished' | 'ResultsProcessed'; // Based on RaceStatus enum
    season: number;
    flag: string;
}

/**
 * Corresponds to F1BettingApp.Application.DTOs.RaceDto
 * Summary of a race, including odds mapping.
 */
export interface RaceDto {
    id: number;
    name: string;
    circuit: string;
    raceDate: Date;
    country: string;
    status: 'Scheduled' | 'InProgress'| 'Finished' | 'ResultsProcessed';
    season: number;
    flag: string;
    // Key: DriverId, Value: Odds
    odds: Record<number, number>; 
}

/**
 * Corresponds to F1BettingApp.Application.DTOs.RaceDetailDto
 * Detailed race information.
 */
export interface RaceDetailDto {
    id: number;
    name: string;
    circuit: string;
    country: string;
    raceDate: Date;
    status: 'Scheduled' | 'InProgress'| 'Finished' | 'ResultsProcessed';
    openF1RaceId: string;
    season: number;
    weather: string;
    trackCondition: string;
    flag: string;
    paddock: string;
    circuitLayout: string;
    sprintRace: string;
    sprintDate: string;
}

/**
 * Utility type for pagination results.
 * @template T The type of items being paginated.
 */
export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
}