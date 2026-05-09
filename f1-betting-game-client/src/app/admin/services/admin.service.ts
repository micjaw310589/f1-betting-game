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
} from '../models/admin.models';

@Injectable({
    providedIn: 'root',
})
export class AdminService {
    private readonly API_URL = `${environment.apiUrl}/Users`;

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

    private handleError(error: any) {
        let errorMessage = 'Unknown error occurred';
        if (error.error instanceof ErrorEvent) {
            errorMessage = `Error: ${error.error.message}`;
        } else {
            errorMessage = `Server error: ${error.status}\nMessage: ${error.message}`;
        }
        console.error(errorMessage);
        return throwError(() => new Error(errorMessage));
    }
}
