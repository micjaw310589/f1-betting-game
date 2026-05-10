import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
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
    private cdr: ChangeDetectorRef,
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
  if (this.registerForm.invalid) return;

  this.isLoading = true;
  this.errorMessage = null;

  const { email, username, password } = this.registerForm.value;

  this.authService.register(email, username, password).subscribe({
    next: (response) => {
      this.isLoading = false;
      if (response.isSuccess) {
        this.successMessage = 'Success!';
        this.cdr.detectChanges(); // Wymuś odświeżenie sukcesu
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
      }
    },
    error: (errMessage) => {
      console.log('Komponent odebrał błąd:', errMessage);
      
      // TO KLUCZOWE:
      this.isLoading = false;      // Odblokuj przycisk
      this.errorMessage = errMessage; // Przypisz tekst błędu
      
      this.cdr.detectChanges();    // WYMUŚ ODŚWIEŻENIE WIDOKU TERAZ
    }
  });
}
}
