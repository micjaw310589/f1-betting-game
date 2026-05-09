import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  isLoading = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  ngOnInit(): void {
    // Note: Removed automatic redirect to allow users to register even if they have an invalid session
    // Users can manually navigate away if they're already logged in
  }

  onSubmit(): void {
  if (this.registerForm.invalid) {
    return;
  }

  this.isLoading = true;
  this.errorMessage = null; // Czyścimy stare błędy przed nową próbą
  this.successMessage = null;

  const { email, username, password } = this.registerForm.value;

  this.authService.register(email, username, password).subscribe({
    next: (response) => {
      // Jeśli backend zwrócił 200 OK, ale z flagą isSuccess: false
      if (response.isSuccess) {
        this.successMessage = 'Registration successful! Redirecting to login...';
        this.isLoading = false;
        setTimeout(() => {
          this.router.navigate(['/auth/login']);
        }, 2000);
      } else {
        this.errorMessage = response.errorMessage || 'Registration failed';
        this.isLoading = false;
      }
    },
    error: (err) => {
      // Obsługa błędów HTTP (np. 409 Conflict, 400 Bad Request)
      // Sprawdzamy czy backend przysłał nam obiekt z polem errorMessage
      this.errorMessage = err.error?.errorMessage || err.message || 'An unexpected error occurred';
      this.isLoading = false;
      console.error('Registration error details:', err);
    }
  });
}
}