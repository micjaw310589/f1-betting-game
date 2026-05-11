// cd f1-betting-game-client
// ng test --include=src/app/auth/register/register.component.spec.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RegisterComponent } from './register.component';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError, Observable } from 'rxjs';
import { RouterTestingModule } from '@angular/router/testing';
import { By } from '@angular/platform-browser';
import { vi, describe, it, expect, beforeEach } from 'vitest';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authService: any;
  let router: Router;

  beforeEach(async () => {
    const authServiceSpy = { register: vi.fn() };

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
    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize register form with empty fields', () => {
    expect(component.registerForm).toBeDefined();
    expect(component.registerForm.controls['email']).toBeDefined();
    expect(component.registerForm.controls['username']).toBeDefined();
    expect(component.registerForm.controls['password']).toBeDefined();
    expect(component.registerForm.controls['email'].value).toBe('');
    expect(component.registerForm.controls['username'].value).toBe('');
    expect(component.registerForm.controls['password'].value).toBe('');
  });

  it('should have required validators on all fields', () => {
    const emailControl = component.registerForm.controls['email'];
    const usernameControl = component.registerForm.controls['username'];
    const passwordControl = component.registerForm.controls['password'];

    emailControl.setValue('');
    usernameControl.setValue('');
    passwordControl.setValue('');
    
    expect(emailControl.valid).toBeFalsy();
    expect(usernameControl.valid).toBeFalsy();
    expect(passwordControl.valid).toBeFalsy();
    expect(emailControl.errors?.['required']).toBeTruthy();
    expect(usernameControl.errors?.['required']).toBeTruthy();
    expect(passwordControl.errors?.['required']).toBeTruthy();
  });

  it('should have email validator on email field', () => {
    const emailControl = component.registerForm.controls['email'];

    emailControl.setValue('invalid-email');
    expect(emailControl.valid).toBeFalsy();
    expect(emailControl.errors?.['email']).toBeTruthy();

    emailControl.setValue('valid@example.com');
    expect(emailControl.valid).toBeTruthy();
    expect(emailControl.errors).toBeNull();
  });

  it('should have minLength validator on username field', () => {
    const usernameControl = component.registerForm.controls['username'];

    usernameControl.setValue('ab');
    expect(usernameControl.valid).toBeFalsy();
    expect(usernameControl.errors?.['minlength']).toBeTruthy();

    usernameControl.setValue('validusername');
    expect(usernameControl.valid).toBeTruthy();
  });

  it('should have minLength validator on password field', () => {
    const passwordControl = component.registerForm.controls['password'];

    passwordControl.setValue('short');
    expect(passwordControl.valid).toBeFalsy();
    expect(passwordControl.errors?.['minlength']).toBeTruthy();

    passwordControl.setValue('longenough');
    expect(passwordControl.valid).toBeTruthy();
  });

  it('should not call authService.register when form is invalid', () => {
    component.registerForm.controls['email'].setValue('');
    component.onSubmit();
    expect(authService.register).not.toHaveBeenCalled();
  });

  it('should call authService.register with correct parameters when form is valid', async () => {
    component.registerForm.controls['email'].setValue('test@example.com');
    component.registerForm.controls['username'].setValue('testuser');
    component.registerForm.controls['password'].setValue('password123');

    authService.register.mockReturnValue(of({ isSuccess: true }));

    component.onSubmit();

    expect(authService.register).toHaveBeenCalledWith('test@example.com', 'testuser', 'password123');
  });

  it('should set errorMessage and isLoading when registration fails', async () => {
    component.registerForm.controls['email'].setValue('test@example.com');
    component.registerForm.controls['username'].setValue('testuser');
    component.registerForm.controls['password'].setValue('password123');

    authService.register.mockReturnValue(of({ isSuccess: false, errorMessage: 'Email already exists' }));

    component.onSubmit();

    expect(component.errorMessage).toBe('Email already exists');
    expect(component.isLoading).toBeFalsy();
  });

  it('should set errorMessage and isLoading when registration throws error', async () => {
    component.registerForm.controls['email'].setValue('test@example.com');
    component.registerForm.controls['username'].setValue('testuser');
    component.registerForm.controls['password'].setValue('password123');

    authService.register.mockReturnValue(throwError(() => new Error('Network error')));

    component.onSubmit();

    expect(component.errorMessage).toBe('Network error');
    expect(component.isLoading).toBeFalsy();
  });

  it('should show loading state during registration', async () => {
    component.registerForm.controls['email'].setValue('test@example.com');
    component.registerForm.controls['username'].setValue('testuser');
    component.registerForm.controls['password'].setValue('password123');

    authService.register.mockReturnValue(new Observable(subscriber => {
      setTimeout(() => {
        subscriber.next({ isSuccess: true });
        subscriber.complete();
      }, 100);
    }));

    component.onSubmit();
    expect(component.isLoading).toBeTruthy();

    await new Promise(resolve => setTimeout(resolve, 110));
    expect(component.isLoading).toBeFalsy();
  });
});