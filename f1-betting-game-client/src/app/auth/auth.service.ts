import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';

interface AuthResponse {
  isSuccess: boolean;
  token?: string;
  refreshToken?: string;
  errorMessage?: string;
  user?: any;
}

interface LoginDto {
  email: string;
  password: string;
}

interface RegisterDto {
  email: string;
  username: string;
  password: string;
}

interface RefreshTokenDto {
  token: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/auth';
  private currentUserSubject: BehaviorSubject<any>;
  public currentUser: Observable<any>;

  constructor(private http: HttpClient, private router: Router) {
    this.currentUserSubject = new BehaviorSubject<any>(JSON.parse(localStorage.getItem('currentUser') || 'null'));
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): any {
    return this.currentUserSubject.value;
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap(response => {
          if (response.isSuccess && response.token && response.refreshToken) {
            // Store user details and jwt token in local storage
            localStorage.setItem('currentUser', JSON.stringify({
              token: response.token,
              refreshToken: response.refreshToken,
              user: response.user
            }));
            this.currentUserSubject.next({
              token: response.token,
              refreshToken: response.refreshToken,
              user: response.user
            });
          }
        }),
        catchError(error => {
          return throwError(() => new Error(error.error?.errorMessage || 'Login failed'));
        })
      );
  }

  register(email: string, username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { email, username, password })
      .pipe(
        catchError(error => {
          return throwError(() => new Error(error.error?.errorMessage || 'Registration failed'));
        })
      );
  }

  refreshToken(): Observable<AuthResponse> {
    const currentUser = this.currentUserValue;
    if (!currentUser?.refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, {
      token: currentUser.token,
      refreshToken: currentUser.refreshToken
    }).pipe(
      tap(response => {
        if (response.isSuccess && response.token && response.refreshToken) {
          // Update stored user with new tokens
          const updatedUser = {
            ...currentUser,
            token: response.token,
            refreshToken: response.refreshToken
          };
          localStorage.setItem('currentUser', JSON.stringify(updatedUser));
          this.currentUserSubject.next(updatedUser);
        }
      }),
      catchError(error => {
        this.logout();
        return throwError(() => new Error(error.error?.errorMessage || 'Token refresh failed'));
      })
    );
  }

  logout(): void {
    // Remove user from local storage
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  isLoggedIn(): boolean {
    return this.currentUserValue !== null;
  }

  getToken(): string | null {
    return this.currentUserValue?.token || null;
  }

  getRefreshToken(): string | null {
    return this.currentUserValue?.refreshToken || null;
  }
}