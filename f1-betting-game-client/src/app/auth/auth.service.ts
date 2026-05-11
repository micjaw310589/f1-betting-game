import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, shareReplay, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

export interface AuthResponse {
  isSuccess: boolean;
  accessToken?: string;
  token?: string;
  refreshToken?: string;
  errorMessage?: string;
  user?: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject: BehaviorSubject<any>;
  public currentUser: Observable<any>;

  constructor(private http: HttpClient, private router: Router) {
    this.currentUserSubject = new BehaviorSubject<any>(JSON.parse(localStorage.getItem('currentUser') || 'null'));
    this.currentUser = this.currentUserSubject.asObservable().pipe(shareReplay(1));
  }

  public get currentUserValue(): any {
    return this.currentUserSubject.value;
  }

  login(email: string, password: string): Observable<AuthResponse> {
    const loginData = { usernameOrEmail: email, password: password };
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginData)
      .pipe(
        tap(response => {
          if (response.isSuccess) {
            this.saveSession(response);
          }
        }),
        catchError(error => throwError(() => new Error(error.error?.errorMessage || 'Login failed')))
      );
  }

register(email: string, username: string, password: string): Observable<AuthResponse> {
  return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { email, username, password })
    .pipe(
      catchError(error => {
        // Wyciągamy czysty tekst błędu z backendu
        const errorMessage = error.error?.errorMessage || error.message || 'Registration failed';
        // Rzucamy go dalej jako prosty błąd
        return throwError(() => errorMessage); 
      })
    );
}

  refreshToken(): Observable<AuthResponse> {
    const currentUser = this.currentUserValue;
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, {
      token: currentUser?.token || currentUser?.accessToken,
      refreshToken: currentUser?.refreshToken
    }).pipe(
      tap(response => {
        if (response.isSuccess) {
          this.saveSession(response);
        }
      }),
      catchError(error => {
        this.logout();
        return throwError(() => new Error('Session expired'));
      })
    );
  }

  private saveSession(response: AuthResponse) {
    const userData = {
      token: response.accessToken || response.token,
      refreshToken: response.refreshToken,
      user: response.user
    };
    localStorage.setItem('currentUser', JSON.stringify(userData));
    this.currentUserSubject.next(userData);
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    this.router.navigate(['/']); 
  }

  isLoggedIn(): boolean {
    return !!this.currentUserValue;
  }

  getToken(): string | null {
    return this.currentUserValue?.token || null;
  }

  getRefreshToken(): string | null {
    return this.currentUserValue?.refreshToken || null;
  }

  isAdmin(): boolean {
    return this.currentUserValue?.user?.isAdmin === true;
  }

  getAuthorizationHeader(): string | null {
    const token = this.getToken();
    return token ? `Bearer ${token}` : null;
  }

  user(): any {
    return this.currentUserValue?.user || null;
  }
}