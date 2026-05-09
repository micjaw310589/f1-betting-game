import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService, LoginDto } from '../services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
    private fb = inject(FormBuilder);
    private authService = inject(AuthService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);

    loginForm = this.fb.group({
        usernameOrEmail: ['', [Validators.required]],
        password: ['', [Validators.required]],
        rememberMe: [false],
    });

    errorMessage = '';
    isLoading = false;
    returnUrl = '/races';

    ngOnInit(): void {
        // If already logged in, redirect to races
        if (this.authService.isLoggedIn()) {
            this.router.navigate(['/races']);
        }

        // Get the return URL from query parameters
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/races';
    }

    onSubmit(): void {
        if (this.loginForm.invalid) {
            this.loginForm.markAllAsTouched();
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';

        const formData: LoginDto = {
            usernameOrEmail: this.loginForm.get('usernameOrEmail')!.value!,
            password: this.loginForm.get('password')!.value!,
            rememberMe: this.loginForm.get('rememberMe')!.value!,
        };

        this.authService.login(formData).subscribe({
            next: () => {
                this.router.navigate([this.returnUrl]);
            },
            error: (error) => {
                this.errorMessage = error.message || 'Login failed. Please check your credentials.';
                this.isLoading = false;
            },
        });
    }
}
