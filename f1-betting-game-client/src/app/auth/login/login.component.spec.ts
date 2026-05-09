import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { FormsModule } from '@angular/forms';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: any;
  let router: any;

  beforeEach(async () => {
    // Create simple mock objects with manual call tracking
    const authServiceSpy = {
      loginCalls: [] as any[],
      login: function(email: string, password: string) {
        this.loginCalls.push({ email, password });
        return of({ isSuccess: false });
      },
      returnValue: function(val: any) {
        this.login = function(email: string, password: string) {
          this.loginCalls.push({ email, password });
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
        LoginComponent,
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
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize login form with empty fields', () => {
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.get('email')?.value).toBe('');
    expect(component.loginForm.get('password')?.value).toBe('');
  });

  it('should make email field required', () => {
    const emailControl = component.loginForm.get('email');
    emailControl?.setValue('');
    expect(emailControl?.valid).toBeFalsy();
    expect(emailControl?.errors?.['required']).toBeTruthy();
  });

  it('should validate email format', () => {
    const emailControl = component.loginForm.get('email');
    emailControl?.setValue('invalid-email');
    expect(emailControl?.valid).toBeFalsy();
    expect(emailControl?.errors?.['email']).toBeTruthy();

    emailControl?.setValue('valid@example.com');
    expect(emailControl?.valid).toBeTruthy();
    expect(emailControl?.errors).toBeNull();
  });

  it('should make password field required', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('');
    expect(passwordControl?.valid).toBeFalsy();
    expect(passwordControl?.errors?.['required']).toBeTruthy();
  });

  it('should validate password minimum length', () => {
    const passwordControl = component.loginForm.get('password');
    passwordControl?.setValue('short');
    expect(passwordControl?.valid).toBeFalsy();
    expect(passwordControl?.errors?.['minlength']).toBeTruthy();

    passwordControl?.setValue('longenough');
    expect(passwordControl?.valid).toBeTruthy();
    expect(passwordControl?.errors).toBeNull();
  });

  it('should not submit form when invalid', () => {
    component.loginForm.get('email')?.setValue('invalid-email');
    component.loginForm.get('password')?.setValue('short');
    expect(component.loginForm.valid).toBeFalsy();

    component.onSubmit();
    expect(authService.loginCalls.length).toBe(0);
  });

  it('should call auth service and navigate on successful login', fakeAsync(() => {
    const mockResponse = { isSuccess: true, accessToken: 'test-token', refreshToken: 'refresh-token' };
    authService.returnValue(of(mockResponse));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('password123');
    expect(component.loginForm.valid).toBeTruthy();

    component.onSubmit();
    tick();

    expect(authService.loginCalls.length).toBe(1);
    expect(authService.loginCalls[0].email).toBe('test@example.com');
    expect(authService.loginCalls[0].password).toBe('password123');
    expect(router.navigateCalls.length).toBe(1);
    expect(router.navigateCalls[0]).toEqual(['/']);
    expect(component.isLoading).toBeFalsy();
    expect(component.errorMessage).toBeNull();
  }));

  it('should handle login error', fakeAsync(() => {
    const mockError = { message: 'Invalid credentials' };
    authService.returnValue(throwError(() => mockError));

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('password123');

    component.onSubmit();
    tick();

    expect(authService.loginCalls.length).toBe(1);
    expect(component.errorMessage).toBe('Invalid credentials');
    expect(component.isLoading).toBeFalsy();
  }));

  it('should show loading state during login', () => {
    authService.returnValue(new Promise(() => {})); // Never resolves

    component.loginForm.get('email')?.setValue('test@example.com');
    component.loginForm.get('password')?.setValue('password123');

    component.onSubmit();
    expect(component.isLoading).toBeTruthy();
  });
});