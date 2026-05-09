import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-register',
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
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // Redirect to home if already logged in
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/']);
    }
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.successMessage = null;

    const { email, username, password } = this.registerForm.value;

    this.authService.register(email, username, password).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.successMessage = 'Registration successful! You can now login.';
          this.isLoading = false;
          // Optionally auto-login after registration
          // this.authService.login(email, password).subscribe(...);
        } else {
          this.errorMessage = response.errorMessage || 'Registration failed';
          this.isLoading = false;
        }
      },
      error: (error) => {
        this.errorMessage = error.message || 'Registration failed';
        this.isLoading = false;
      }
    });
  }
}