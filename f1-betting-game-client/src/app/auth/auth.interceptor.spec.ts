import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HTTP_INTERCEPTORS, HttpClient, HttpRequest } from '@angular/common/http';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';

// Simple fail function for testing
function fail(message: string): never {
  throw new Error(message);
}

describe('AuthInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: any;

  beforeEach(() => {
    const authServiceSpy = {
      getTokenCalls: [] as any[],
      getToken: function() { return null; },
      getRefreshToken: function() { return null; },
      refreshToken: function() { return of({ isSuccess: false }); }
    };

    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        RouterTestingModule
      ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        {
          provide: HTTP_INTERCEPTORS,
          useClass: AuthInterceptor,
          multi: true
        }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    const interceptor: AuthInterceptor = TestBed.inject(AuthInterceptor);
    expect(interceptor).toBeTruthy();
  });

  it('should add Authorization header when token is available', () => {
    // Arrange
    const testToken = 'test-token-123';
    authService.getToken = function() { return testToken; };

    // Act
    http.get('/api/test').subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/test');
    expect(httpRequest.request.headers.has('Authorization')).toBe(true);
    expect(httpRequest.request.headers.get('Authorization')).toBe(`Bearer ${testToken}`);
  });

  it('should not add Authorization header for login requests', () => {
    // Arrange
    const testToken = 'test-token-123';
    authService.getToken = function() { return testToken; };

    // Act
    http.post('/api/auth/login', {}).subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/auth/login');
    expect(httpRequest.request.headers.has('Authorization')).toBe(false);
  });

  it('should not add Authorization header for register requests', () => {
    // Arrange
    const testToken = 'test-token-123';
    authService.getToken = function() { return testToken; };

    // Act
    http.post('/api/auth/register', {}).subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/auth/register');
    expect(httpRequest.request.headers.has('Authorization')).toBe(false);
  });

  it('should not add Authorization header when no token is available', () => {
    // Arrange
    authService.getToken = function() { return null; };

    // Act
    http.get('/api/test').subscribe();

    // Assert
    const httpRequest = httpMock.expectOne('/api/test');
    expect(httpRequest.request.headers.has('Authorization')).toBe(false);
  });

  it('should handle 401 error and attempt token refresh', () => {
    // Arrange
    const testToken = 'test-token-123';
    const refreshToken = 'refresh-token-456';
    let refreshTokenCalled = false;
    authService.getToken = function() { return testToken; };
    authService.getRefreshToken = function() { return refreshToken; };
    authService.refreshToken = function() {
      refreshTokenCalled = true;
      return of({
        isSuccess: true,
        accessToken: 'new-token-789',
        refreshToken: 'new-refresh-token'
      });
    };

    // Act
    http.get('/api/protected').subscribe({
      next: () => fail('should have failed with 401 error'),
      error: (error) => {
        expect(error.status).toBe(401);
      }
    });

    // First request fails with 401
    const firstRequest = httpMock.expectOne('/api/protected');
    firstRequest.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    // Verify that refreshToken was called
    expect(refreshTokenCalled).toBe(true);
  });
});