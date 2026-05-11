// cd f1-betting-game-client
// ng test --include=src/app/auth/login/login.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError, Observable } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';
import { By } from '@angular/platform-browser';
import { vi, describe, it, expect, beforeEach } from 'vitest';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: any;
  let router: Router;

  beforeEach(async () => {
    const authServiceSpy = { login: vi.fn() };

    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule, 
        RouterTestingModule
      ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy }
      ]
    }).compileComponents();

    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
    
    vi.spyOn(router, 'navigate');
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize login form with empty fields', () => {
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.controls['email']).toBeDefined();
    expect(component.loginForm.controls['password']).toBeDefined();
    expect(component.loginForm.controls['email'].value).toBe('');
    expect(component.loginForm.controls['password'].value).toBe('');
  });

  it('should have required validators on email and password fields', () => {
    const emailControl = component.loginForm.controls['email'];
    const passwordControl = component.loginForm.controls['password'];

    emailControl.setValue('');
    passwordControl.setValue('');
    expect(emailControl.valid).toBe(false);
    expect(passwordControl.valid).toBe(false);
    expect(emailControl.errors?.['required']).toBeTruthy();
    expect(passwordControl.errors?.['required']).toBeTruthy();
  });

  it('should have email validator on email field', () => {
    const emailControl = component.loginForm.controls['email'];

    emailControl.setValue('invalid-email');
    expect(emailControl.valid).toBe(false);
    expect(emailControl.errors?.['email']).toBeTruthy();

    emailControl.setValue('valid@example.com');
    expect(emailControl.valid).toBe(true);
    expect(emailControl.errors).toBeNull();
  });

  it('should have minLength validator on password field', () => {
    const passwordControl = component.loginForm.controls['password'];

    passwordControl.setValue('short');
    expect(passwordControl.valid).toBe(false);
    expect(passwordControl.errors?.['minlength']).toBeTruthy();

    passwordControl.setValue('longenough');
    expect(passwordControl.valid).toBe(true);
    expect(passwordControl.errors).toBeNull();
  });

  it('should not call authService.login when form is invalid', () => {
    component.loginForm.controls['email'].setValue('');
    component.loginForm.controls['password'].setValue('');

    component.onSubmit();

    expect(authService.login).not.toHaveBeenCalled();
  });

  it('should call authService.login with correct parameters when form is valid', async () => {
    component.loginForm.controls['email'].setValue('test@example.com');
    component.loginForm.controls['password'].setValue('password123');

    const mockResponse = { isSuccess: true };
    authService.login.mockReturnValue(of(mockResponse));

    component.onSubmit();

    expect(authService.login).toHaveBeenCalledWith('test@example.com', 'password123');
  });

  it('should set errorMessage and isLoading when login fails', async () => {
    component.loginForm.controls['email'].setValue('test@example.com');
    component.loginForm.controls['password'].setValue('password123');

    const mockResponse = { isSuccess: false, errorMessage: 'Invalid credentials' };
    authService.login.mockReturnValue(of(mockResponse));

    component.onSubmit();

    expect(component.errorMessage).toBe('Invalid credentials');
    expect(component.isLoading).toBe(false);
  });

  it('should set errorMessage and isLoading when login throws error', async () => {
    component.loginForm.controls['email'].setValue('test@example.com');
    component.loginForm.controls['password'].setValue('password123');

    authService.login.mockReturnValue(throwError(() => new Error('Network error')));

    component.onSubmit();

    expect(component.errorMessage).toBe('Network error');
    expect(component.isLoading).toBe(false);
  });

  it('should navigate to home page when login is successful', async () => {
    component.loginForm.controls['email'].setValue('test@example.com');
    component.loginForm.controls['password'].setValue('password123');

    const mockResponse = { isSuccess: true };
    authService.login.mockReturnValue(of(mockResponse));

    component.onSubmit();

    expect(router.navigate).toHaveBeenCalledWith(['/']);
    expect(component.isLoading).toBe(false);
  });

  it('should show loading state during login', async () => {
    component.loginForm.controls['email'].setValue('test@example.com');
    component.loginForm.controls['password'].setValue('password123');

    authService.login.mockReturnValue(new Observable(subscriber => {
      setTimeout(() => {
        subscriber.next({ isSuccess: true });
        subscriber.complete();
      }, 100);
    }));

    component.onSubmit();
    expect(component.isLoading).toBe(true);

    await new Promise(resolve => setTimeout(resolve, 110));
    
    expect(component.isLoading).toBe(false);
  });
});