import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserDto {
    id: number;
    username: string;
    email: string;
    points: number;
}

export interface AuthResponseDto {
    isSuccess: boolean;
    errorMessage?: string;
    accessToken: string;
    refreshToken: string;
    tokenType: string;
    accessTokenExpiration: number;
    refreshTokenExpiration: number;
    user?: UserDto;
}

export interface RegisterDto {
    username: string;
    email: string;
    password: string;
    profileImageUrl?: string;
}

export interface LoginDto {
    usernameOrEmail: string;
    password: string;
    rememberMe: boolean;
}

export interface RefreshTokenDto {
    refreshToken: string;
}

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private readonly API_URL = `${environment.apiUrl}/auth`;

    // Signals for reactive state management
    private readonly _accessToken = signal<string>(this.loadToken('accessToken') || '');
    private readonly _refreshToken = signal<string>(this.loadToken('refreshToken') || '');
    private readonly _user = signal<UserDto | null>(this.loadUser());

    // Computed signals
    readonly isLoggedIn = computed(() => !!this._accessToken());
    readonly isAdmin = computed(() => {
        const token = this._accessToken();
        if (!token) return false;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'Admin';
        } catch {
            return false;
        }
    });
    readonly user = computed(() => this._user());

    constructor(private http: HttpClient) {}

    // --- Token Management ---

    private saveToken(key: string, value: string): void {
        localStorage.setItem(key, value);
    }

    private loadToken(key: string): string | null {
        return localStorage.getItem(key);
    }

    private removeToken(key: string): void {
        localStorage.removeItem(key);
    }

    private saveUser(user: UserDto): void {
        localStorage.setItem('user', JSON.stringify(user));
        this._user.set(user);
    }

    private loadUser(): UserDto | null {
        const stored = localStorage.getItem('user');
        return stored ? JSON.parse(stored) : null;
    }

    private removeUser(): void {
        localStorage.removeItem('user');
        this._user.set(null);
    }

    // --- Authentication Methods ---

    register(dto: RegisterDto): Observable<AuthResponseDto> {
        return this.http.post<AuthResponseDto>(`${this.API_URL}/register`, dto).pipe(
            tap((response) => {
                if (response.isSuccess && response.accessToken) {
                    this._accessToken.set(response.accessToken);
                    this._refreshToken.set(response.refreshToken);
                    this.saveToken('accessToken', response.accessToken);
                    this.saveToken('refreshToken', response.refreshToken);
                    if (response.user) {
                        this.saveUser(response.user);
                    }
                }
            }),
            catchError(this.handleError)
        );
    }

    login(dto: LoginDto): Observable<AuthResponseDto> {
        return this.http.post<AuthResponseDto>(`${this.API_URL}/login`, dto).pipe(
            tap((response) => {
                if (response.isSuccess && response.accessToken) {
                    this._accessToken.set(response.accessToken);
                    this._refreshToken.set(response.refreshToken);
                    this.saveToken('accessToken', response.accessToken);
                    this.saveToken('refreshToken', response.refreshToken);
                    if (response.user) {
                        this.saveUser(response.user);
                    }
                }
            }),
            catchError(this.handleError)
        );
    }

    refreshToken(dto: RefreshTokenDto): Observable<AuthResponseDto> {
        return this.http.post<AuthResponseDto>(`${this.API_URL}/refresh-token`, dto).pipe(
            tap((response) => {
                if (response.isSuccess && response.accessToken) {
                    this._accessToken.set(response.accessToken);
                    this._refreshToken.set(response.refreshToken);
                    this.saveToken('accessToken', response.accessToken);
                    this.saveToken('refreshToken', response.refreshToken);
                }
            }),
            catchError(this.handleError)
        );
    }

    logout(): void {
        this._accessToken.set('');
        this._refreshToken.set('');
        this.removeToken('accessToken');
        this.removeToken('refreshToken');
        this.removeUser();
    }

    getCurrentUser(): Observable<UserDto> {
        return this.http.get<UserDto>(`${this.API_URL}/me`).pipe(
            tap((user) => this.saveUser(user)),
            catchError(() => throwError(() => new Error('Failed to fetch user profile')))
        );
    }

    // --- HTTP Interceptor Token Helper ---

    getAuthorizationHeader(): string {
        const token = this._accessToken();
        return token ? `Bearer ${token}` : '';
    }

    // --- Error Handling ---

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
