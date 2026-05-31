import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    AdminUserDto,
    AdjustUserPointsDto,
    AdjustPointsResultDto,
    ChangeUserStatusDto,
    PagedResult,
    SyncResultDto,
    AdminRaceDto,
    UpdateRaceMetadataDto,
    RaceResultDto,
    OverrideRaceResultDto,
    DriverDto,
    AdminBetResponseDto,
    CreateBetDto,
    UpdateBetDto,
    BetStatus,
    QuestDefinitionDto,
    CreateQuestDefinitionDto,
    UpdateQuestDefinitionDto,
    ResetWeekResponseDto,
    CompletedCountResponseDto,
} from '../models/admin.models';

@Injectable({
    providedIn: 'root',
})
export class AdminService {
    private readonly API_URL = `${environment.apiUrl}/Users`;
    private readonly ADMIN_API_URL = `${environment.apiUrl}/admin`;

    constructor(private http: HttpClient) {}

    /**
     * Gets all users with optional filtering and pagination (admin only).
     */
    getAllUsers(
        page: number = 1,
        pageSize: number = 20,
        filterIsActive?: boolean,
        searchTerm?: string
    ): Observable<PagedResult<AdminUserDto>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (filterIsActive !== undefined) {
            params = params.set('filterIsActive', filterIsActive.toString());
        }
        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.http
            .get<PagedResult<AdminUserDto>>(`${this.API_URL}/admin/users`, { params })
            .pipe(catchError(this.handleError));
    }

    /**
     * Adjusts a user's point balance (admin only).
     * Positive points adds, negative points removes.
     */
    adjustUserPoints(
        userId: number,
        dto: AdjustUserPointsDto
    ): Observable<AdjustPointsResultDto> {
        return this.http
            .patch<AdjustPointsResultDto>(
                `${this.API_URL}/admin/users/${userId}/points`,
                dto
            )
            .pipe(catchError(this.handleError));
    }

    /**
     * Changes a user's account status - suspend/reactivate (admin only).
     */
    changeUserStatus(
        userId: number,
        dto: ChangeUserStatusDto
    ): Observable<AdminUserDto> {
        return this.http
            .patch<AdminUserDto>(
                `${this.API_URL}/admin/users/${userId}/status`,
                dto
            )
            .pipe(catchError(this.handleError));
    }

    // ========================
    // System Management Methods
    // ========================

    /**
     * Manually triggers OpenF1 data synchronization (admin only).
     */
    triggerSync(): Observable<SyncResultDto> {
        return this.http
            .post<SyncResultDto>(`${this.ADMIN_API_URL}/sync`, {})
            .pipe(catchError(this.handleError));
    }

    /**
     * Gets all races with override status (admin only).
     */
    getAllRaces(): Observable<AdminRaceDto[]> {
        return this.http
            .get<AdminRaceDto[]>(`${this.ADMIN_API_URL}/races`)
            .pipe(catchError(this.handleError));
    }

    /**
     * Updates race metadata (name, date, status, circuit, country) (admin only).
     */
    updateRaceMetadata(
        raceId: number,
        dto: UpdateRaceMetadataDto
    ): Observable<{ message: string; raceId: number; isManuallyOverridden: boolean }> {
        return this.http
            .put<{
                message: string;
                raceId: number;
                isManuallyOverridden: boolean;
            }>(`${this.ADMIN_API_URL}/races/${raceId}/metadata`, dto)
            .pipe(catchError(this.handleError));
    }

    /**
     * Gets race results for a specific race (admin only).
     */
    getRaceResults(raceId: number): Observable<RaceResultDto> {
        return this.http
            .get<RaceResultDto>(`${this.ADMIN_API_URL}/races/${raceId}/results`)
            .pipe(catchError(this.handleError));
    }

    /**
     * Overrides race results manually (admin only).
     */
    overrideRaceResults(
        raceId: number,
        dto: OverrideRaceResultDto
    ): Observable<{ message: string; raceId: number; positionsCount: number; isManuallyOverridden: boolean }> {
        return this.http
            .put<{
                message: string;
                raceId: number;
                positionsCount: number;
                isManuallyOverridden: boolean;
            }>(`${this.ADMIN_API_URL}/races/${raceId}/results`, dto)
            .pipe(catchError(this.handleError));
    }

    /**
     * Gets all available drivers (for admin override dropdowns).
     */
    getAllDrivers(): Observable<DriverDto[]> {
        return this.http
            .get<DriverDto[]>(`${environment.apiUrl}/races/drivers`)
            .pipe(catchError(this.handleError));
    }

    /**
     * Creates a new race (admin only).
     */
    createRace(dto: { name: string; date: string; circuit: string; country: string; season: number }): Observable<AdminRaceDto> {
        return this.http
            .post<AdminRaceDto>(`${this.ADMIN_API_URL}/races`, dto)
            .pipe(catchError(this.handleError));
    }

    /**
     * Deletes a race (admin only).
     */
    deleteRace(raceId: number): Observable<void> {
        return this.http
            .delete<void>(`${this.ADMIN_API_URL}/races/${raceId}`)
            .pipe(catchError(this.handleError));
    }

    // ========================
    // Bet Management Methods
    // ========================

    /**
     * Gets all bets with pagination and optional filtering (admin only).
     */
    getAllBets(
        page: number = 1,
        pageSize: number = 20,
        filterStatus?: BetStatus | null,
        searchTerm?: string
    ): Observable<PagedResult<AdminBetResponseDto>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (filterStatus) {
            params = params.set('filterStatus', filterStatus);
        }
        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.http
            .get<PagedResult<AdminBetResponseDto>>(`${this.ADMIN_API_URL}/bets`, { params })
            .pipe(catchError(this.handleError));
    }

    /**
     * Creates a new bet on behalf of a user (admin only).
     */
    createBet(dto: CreateBetDto): Observable<AdminBetResponseDto> {
        return this.http
            .post<AdminBetResponseDto>(`${this.ADMIN_API_URL}/bets`, dto)
            .pipe(catchError(this.handleError));
    }

    /**
     * Updates an existing bet (admin only). Supports partial updates.
     */
    updateBet(
        betId: number,
        dto: UpdateBetDto
    ): Observable<AdminBetResponseDto> {
        return this.http
            .put<AdminBetResponseDto>(`${this.ADMIN_API_URL}/bets/${betId}`, dto)
            .pipe(catchError(this.handleError));
    }

    /**
     * Deletes (cancels) a bet (admin only). Only works on pending bets.
     */
    deleteBet(betId: number): Observable<void> {
        return this.http
            .delete<void>(`${this.ADMIN_API_URL}/bets/${betId}`)
            .pipe(catchError(this.handleError));
    }

    // ========================
    // Quest Management Methods
    // ========================

    /**
     * Gets a paginated, filtered, and searchable list of quest definitions (admin only).
     */
    getAllQuestDefinitions(
        page: number = 1,
        pageSize: number = 20,
        isActive?: boolean | null,
        searchTerm?: string
    ): Observable<PagedResult<QuestDefinitionDto>> {
        let params = new HttpParams()
            .set('page', page.toString())
            .set('pageSize', pageSize.toString());

        if (isActive !== undefined && isActive !== null) {
            params = params.set('isActive', isActive.toString());
        }
        if (searchTerm) {
            params = params.set('searchTerm', searchTerm);
        }

        return this.http
            .get<PagedResult<QuestDefinitionDto>>(`${this.ADMIN_API_URL}/quest-definitions/paged`, { params })
            .pipe(catchError(this.handleError));
    }

    /**
     * Creates a new quest definition (admin only).
     */
    createQuestDefinition(dto: CreateQuestDefinitionDto): Observable<QuestDefinitionDto> {
        return this.http
            .post<QuestDefinitionDto>(`${this.ADMIN_API_URL}/quest-definitions`, dto)
            .pipe(catchError(this.handleError));
    }

    /**
     * Updates an existing quest definition (admin only).
     */
    updateQuestDefinition(id: number, dto: UpdateQuestDefinitionDto): Observable<QuestDefinitionDto> {
        return this.http
            .put<QuestDefinitionDto>(`${this.ADMIN_API_URL}/quest-definitions/${id}`, dto)
            .pipe(catchError(this.handleError));
    }

    /**
     * Deletes a quest definition (admin only).
     */
    deleteQuestDefinition(id: number): Observable<void> {
        return this.http
            .delete<void>(`${this.ADMIN_API_URL}/quest-definitions/${id}`)
            .pipe(catchError(this.handleError));
    }

    /**
     * Toggles a quest's active status (admin only).
     */
    toggleQuestActive(id: number, isActive: boolean): Observable<QuestDefinitionDto> {
        return this.http
            .patch<QuestDefinitionDto>(`${this.ADMIN_API_URL}/quest-definitions/${id}/active`, { isActive })
            .pipe(catchError(this.handleError));
    }

    /**
     * Resets all weekly quest progress for the current week (admin only).
     */
    resetWeeklyQuests(): Observable<ResetWeekResponseDto> {
        return this.http
            .post<ResetWeekResponseDto>(`${this.ADMIN_API_URL}/quest-definitions/reset-week`, {})
            .pipe(catchError(this.handleError));
    }

    /**
     * Gets the completion count for a specific quest (admin only).
     */
    getQuestCompletedCount(questId: string): Observable<CompletedCountResponseDto> {
        return this.http
            .get<CompletedCountResponseDto>(`${this.ADMIN_API_URL}/quest-definitions/${questId}/completed-count`)
            .pipe(catchError(this.handleError));
    }

    private handleError(error: any) {
        let errorMessage = 'Unknown error occurred';
        if (error.error && error.error.message) {
            errorMessage = error.error.message;
        } else if (error.message) {
            errorMessage = error.message;
        } else {
            errorMessage = `Server error: ${error.status}`;
        }
        console.error('AdminService error:', errorMessage);
        return throwError(() => new Error(errorMessage));
    }
}
