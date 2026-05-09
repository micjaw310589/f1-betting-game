import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { Router } from '@angular/router';

// Declare Jasmine functions for TypeScript
declare function spyOn(obj: any, method: string): any;
declare function fail(message?: string): never;

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [AuthService]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return currentUser from localStorage', () => {
    const mockUser = { token: 'test-token', user: { username: 'testuser' } };
    localStorage.setItem('currentUser', JSON.stringify(mockUser));

    const currentUser = service.currentUserValue;
    expect(currentUser).toEqual(mockUser);
  });

  it('should return null if no user in localStorage', () => {
    localStorage.removeItem('currentUser');
    const currentUser = service.currentUserValue;
    expect(currentUser).toBeNull();
  });

  it('should login successfully and store user data', () => {
    const mockResponse = {
      isSuccess: true,
      token: 'test-access-token',
      refreshToken: 'test-refresh-token',
      user: { username: 'testuser', email: 'test@example.com' }
    };

    service.login('test@example.com', 'password123').subscribe(response => {
      expect(response.isSuccess).toBe(true);
      expect(localStorage.getItem('currentUser')).not.toBeNull();

      const storedUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
      expect(storedUser.token).toBe('test-access-token');
      expect(storedUser.refreshToken).toBe('test-refresh-token');
    });

    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should handle login error', () => {
    const mockError = { errorMessage: 'Invalid credentials' };

    service.login('wrong@example.com', 'wrongpassword').subscribe(
      response => {
        expect(response.isSuccess).toBe(false);
        expect(response.errorMessage).toBe('Invalid credentials');
      },
      error => {
        fail('should not throw error');
      }
    );

    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush({ isSuccess: false, errorMessage: 'Invalid credentials' });
  });

  it('should register successfully', () => {
    const mockResponse = {
      isSuccess: true,
      user: { username: 'newuser', email: 'new@example.com' }
    };

    service.register('new@example.com', 'newuser', 'Password123!').subscribe(response => {
      expect(response.isSuccess).toBe(true);
    });

    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should handle registration error', () => {
    service.register('duplicate@example.com', 'user', 'password').subscribe(
      response => {
        expect(response.isSuccess).toBe(false);
      },
      error => {
        fail('should not throw error');
      }
    );

    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.method).toBe('POST');
    req.error(new ErrorEvent('error'), { status: 400, statusText: 'Bad Request' });
  });

  it('should logout and clear localStorage', () => {
    // Set up initial state
    const mockUser = { token: 'test-token', user: { username: 'testuser' } };
    localStorage.setItem('currentUser', JSON.stringify(mockUser));

    // Spy on router navigate
    const navigateSpy = spyOn(service['router'], 'navigate');

    // Call logout
    service.logout();

    // Verify
    expect(localStorage.getItem('currentUser')).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });

  it('should return true if user is logged in', () => {
    const mockUser = { token: 'test-token', user: { username: 'testuser' } };
    localStorage.setItem('currentUser', JSON.stringify(mockUser));

    expect(service.isLoggedIn()).toBe(true);
  });

  it('should return false if user is not logged in', () => {
    localStorage.removeItem('currentUser');
    expect(service.isLoggedIn()).toBe(false);
  });

  it('should return token if user is logged in', () => {
    const mockUser = { token: 'test-token', user: { username: 'testuser' } };
    localStorage.setItem('currentUser', JSON.stringify(mockUser));

    const token = service.getToken();
    expect(token).toBe('test-token');
  });

  it('should return null if no token', () => {
    localStorage.removeItem('currentUser');
    const token = service.getToken();
    expect(token).toBeNull();
  });

  it('should return refresh token if user is logged in', () => {
    const mockUser = { token: 'test-token', refreshToken: 'test-refresh', user: { username: 'testuser' } };
    localStorage.setItem('currentUser', JSON.stringify(mockUser));

    const refreshToken = service.getRefreshToken();
    expect(refreshToken).toBe('test-refresh');
  });

  it('should return null if no refresh token', () => {
    localStorage.removeItem('currentUser');
    const refreshToken = service.getRefreshToken();
    expect(refreshToken).toBeNull();
  });
});