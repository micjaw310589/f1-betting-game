// cd f1-betting-game-client
//ng test --include=src/app/auth/auth.interceptor.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';

describe('AuthInterceptor', () => {
  let interceptor: AuthInterceptor;
  let authService: any;
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;

  beforeEach(() => {
    TestBed.resetTestingModule();

    const authServiceSpy = { 
      getToken: vi.fn(), 
      refreshToken: vi.fn(), 
      logout: vi.fn() 
    };

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [
        AuthInterceptor,
        { provide: AuthService, useValue: authServiceSpy },
        {
          provide: HTTP_INTERCEPTORS,
          useClass: AuthInterceptor,
          multi: true
        }
      ]
    });

    authService = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
    interceptor = TestBed.inject(AuthInterceptor);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(interceptor).toBeTruthy();
  });

  it('should add Authorization header when token is available', () => {
    // Arrange
    const testToken = 'test-token-123';
    authService.getToken.mockReturnValue(testToken);

    // Act
    httpClient.get('/api/test').subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/test');
    expect(httpRequest.request.headers.has('Authorization')).toBeTruthy();
    expect(httpRequest.request.headers.get('Authorization')).toBe('Bearer test-token-123');
    httpRequest.flush({});
  });

  it('should not add Authorization header when no token is available', () => {
    // Arrange
    authService.getToken.mockReturnValue(null);

    // Act
    httpClient.get('/api/test').subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/test');
    expect(httpRequest.request.headers.has('Authorization')).toBeFalsy();
    httpRequest.flush({});
  });

  it('should not add Authorization header for login requests', () => {
    // Arrange
    const testToken = 'test-token-123';
    authService.getToken.mockReturnValue(testToken);

    // Act
    httpClient.post('/api/auth/login', {}).subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/auth/login');
    expect(httpRequest.request.headers.has('Authorization')).toBeFalsy();
    httpRequest.flush({});
  });

  it('should not add Authorization header for register requests', () => {
    // Arrange
    const testToken = 'test-token-123';
    authService.getToken.mockReturnValue(testToken);

    // Act
    httpClient.post('/api/auth/register', {}).subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/auth/register');
    expect(httpRequest.request.headers.has('Authorization')).toBeFalsy();
    httpRequest.flush({});
  });

  it('should handle 401 error and attempt token refresh', () => {
    // Arrange
    const testToken = 'expired-token';
    authService.getToken.mockReturnValue(testToken);
    authService.refreshToken.mockReturnValue(of({
      isSuccess: true,
      token: 'new-token-456'
    }));

    // Act
    httpClient.get('/api/protected').subscribe();

    // Assert
    const firstRequest = httpMock.expectOne('/api/protected');
    firstRequest.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authService.refreshToken).toHaveBeenCalled();

    const retryRequest = httpMock.expectOne('/api/protected');
    retryRequest.flush({});
  });

  it('should add Authorization header with new token after successful refresh', () => {
    // Arrange
    const expiredToken = 'expired-token';
    const newToken = 'new-token-456';
    authService.getToken.mockReturnValue(expiredToken);
    authService.refreshToken.mockReturnValue(of({
      isSuccess: true,
      token: newToken
    }));

    // Act
    httpClient.get('/api/protected').subscribe();

    // Assert
    const firstRequest = httpMock.expectOne('/api/protected');
    firstRequest.flush({}, { status: 401, statusText: 'Unauthorized' });

    const secondRequest = httpMock.expectOne('/api/protected');
    expect(secondRequest.request.headers.get('Authorization')).toBe('Bearer new-token-456');
    secondRequest.flush({});
  });

  it('should logout when token refresh fails', () => {
    // Arrange
    const expiredToken = 'expired-token';
    authService.getToken.mockReturnValue(expiredToken);
    authService.refreshToken.mockReturnValue(throwError(() => new Error('Refresh failed')));

    // Act
    httpClient.get('/api/protected').subscribe({
      error: () => {}
    });

    // Assert
    const firstRequest = httpMock.expectOne('/api/protected');
    firstRequest.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(authService.logout).toHaveBeenCalled();
  });

  it('should pass through non-401 errors', () => {
    // Arrange
    const testToken = 'test-token-123';
    authService.getToken.mockReturnValue(testToken);

    // Act
    httpClient.get('/api/test').subscribe({
      error: (error) => {
        expect(error.status).toBe(500);
      }
    });

    // Assert
    const httpRequest = httpMock.expectOne('/api/test');
    httpRequest.flush({}, { status: 500, statusText: 'Internal Server Error' });
  });
});