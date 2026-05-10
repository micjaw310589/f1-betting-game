import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { RegisterComponent } from './register.component';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authService: any;
  let router: any;

  beforeEach(async () => {
    // Create simple mock objects with manual call tracking
    const authServiceSpy = {
      registerCalls: [] as any[],
      register: function(email: string, username: string, password: string) {
        this.registerCalls.push({ email, username, password });
        return of({ isSuccess: false });
      },
      returnValue: function(val: any) {
        this.register = function(email: string, username: string, password: string) {
          this.registerCalls.push({ email, username, password });
          return val;
        };
        return this;
      }
    };
    const routerSpy = {
      navigateCalls: [] as any[],
      navigate: function(path: any) {
        this.navigateCalls.push(path);
      }
    };

    await TestBed.configureTestingModule({
      imports: [
        RegisterComponent,
        ReactiveFormsModule,
        HttpClientTestingModule,
        RouterTestingModule
      ],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize register form with empty fields', () => {
    expect(component.registerForm).toBeDefined();
    expect(component.registerForm.get('email')?.value).toBe('');
    expect(component.registerForm.get('username')?.value).toBe('');
    expect(component.registerForm.get('password')?.value).toBe('');
  });

  it('should make email field required', () => {
    const emailControl = component.registerForm.get('email');
    emailControl?.setValue('');
    expect(emailControl?.valid).toBeFalsy();
    expect(emailControl?.errors?.['required']).toBeTruthy();
  });

  it('should validate email format', () => {
    const emailControl = component.registerForm.get('email');
    emailControl?.setValue('invalid-email');
    expect(emailControl?.valid).toBeFalsy();
    expect(emailControl?.errors?.['email']).toBeTruthy();

    emailControl?.setValue('valid@example.com');
    expect(emailControl?.valid).toBeTruthy();
    expect(emailControl?.errors).toBeNull();
  });

  it('should make username field required', () => {
    const usernameControl = component.registerForm.get('username');
    usernameControl?.setValue('');
    expect(usernameControl?.valid).toBeFalsy();
    expect(usernameControl?.errors?.['required']).toBeTruthy();
  });

  it('should validate username minimum length', () => {
    const usernameControl = component.registerForm.get('username');
    usernameControl?.setValue('ab');
    expect(usernameControl?.valid).toBeFalsy();
    expect(usernameControl?.errors?.['minlength']).toBeTruthy();

    usernameControl?.setValue('validuser');
    expect(usernameControl?.valid).toBeTruthy();
    expect(usernameControl?.errors).toBeNull();
  });

  it('should make password field required', () => {
    const passwordControl = component.registerForm.get('password');
    passwordControl?.setValue('');
    expect(passwordControl?.valid).toBeFalsy();
    expect(passwordControl?.errors?.['required']).toBeTruthy();
  });

  it('should validate password minimum length', () => {
    const passwordControl = component.registerForm.get('password');
    passwordControl?.setValue('short');
    expect(passwordControl?.valid).toBeFalsy();
    expect(passwordControl?.errors?.['minlength']).toBeTruthy();

    passwordControl?.setValue('longenough');
    expect(passwordControl?.valid).toBeTruthy();
    expect(passwordControl?.errors).toBeNull();
  });

  it('should not submit form when invalid', () => {
    component.registerForm.get('email')?.setValue('invalid-email');
    component.registerForm.get('username')?.setValue('ab');
    component.registerForm.get('password')?.setValue('short');
    expect(component.registerForm.valid).toBeFalsy();

    component.onSubmit();
    expect(authService.registerCalls.length).toBe(0);
  });

  it('should call auth service on successful registration', fakeAsync(() => {
    const mockResponse = { isSuccess: true };
    authService.returnValue(of(mockResponse));

    component.registerForm.get('email')?.setValue('test@example.com');
    component.registerForm.get('username')?.setValue('testuser');
    component.registerForm.get('password')?.setValue('password123');
    expect(component.registerForm.valid).toBeTruthy();

    component.onSubmit();
    tick();

    expect(authService.registerCalls.length).toBe(1);
    expect(authService.registerCalls[0].email).toBe('test@example.com');
    expect(authService.registerCalls[0].username).toBe('testuser');
    expect(authService.registerCalls[0].password).toBe('password123');
    expect(component.successMessage).toBe('Registration successful! You can now login.');
    expect(component.isLoading).toBeFalsy();
  }));

  it('should handle registration error', fakeAsync(() => {
    const mockError = { message: 'Registration failed' };
    authService.returnValue(throwError(() => mockError));

    component.registerForm.get('email')?.setValue('test@example.com');
    component.registerForm.get('username')?.setValue('testuser');
    component.registerForm.get('password')?.setValue('password123');

    component.onSubmit();
    tick();

    expect(authService.registerCalls.length).toBe(1);
    expect(component.errorMessage).toBe('Registration failed');
    expect(component.isLoading).toBeFalsy();
  }));

  it('should show loading state during registration', () => {
    authService.returnValue(new Promise(() => {})); // Never resolves

    component.registerForm.get('email')?.setValue('test@example.com');
    component.registerForm.get('username')?.setValue('testuser');
    component.registerForm.get('password')?.setValue('password123');

    component.onSubmit();
    expect(component.isLoading).toBeTruthy();
  });
});