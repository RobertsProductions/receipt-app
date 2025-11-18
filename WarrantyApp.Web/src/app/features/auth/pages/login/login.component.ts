import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { AuthService } from '../../../../services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    ButtonComponent,
    InputComponent,
    CardComponent
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loginForm: FormGroup;
  loading: boolean = false;
  show2FAInput: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toast: ToastService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, this.passwordValidator.bind(this)]],
      twoFactorCode: ['']
    });
  }

  // Custom validator to match backend password requirements
  private passwordValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    if (!password) return null; // Let required validator handle empty

    const errors: ValidationErrors = {};
    
    if (password.length < 6) {
      errors['minlength'] = { requiredLength: 6, actualLength: password.length };
    }
    if (!/[A-Z]/.test(password)) {
      errors['requiresUppercase'] = true;
    }
    if (!/[a-z]/.test(password)) {
      errors['requiresLowercase'] = true;
    }
    if (!/[0-9]/.test(password)) {
      errors['requiresDigit'] = true;
    }

    return Object.keys(errors).length > 0 ? errors : null;
  }

  get emailError(): string | undefined {
    const control = this.loginForm.get('email');
    if (control?.touched && control?.invalid) {
      if (control.errors?.['required']) return 'Email is required';
      if (control.errors?.['email']) return 'Invalid email format';
    }
    return undefined;
  }

  get passwordError(): string | undefined {
    const control = this.loginForm.get('password');
    if (control?.touched && control?.invalid) {
      if (control.errors?.['required']) return 'Password is required';
      if (control.errors?.['minlength']) return 'Password must be at least 6 characters';
      if (control.errors?.['requiresUppercase']) return 'Password must contain at least one uppercase letter';
      if (control.errors?.['requiresLowercase']) return 'Password must contain at least one lowercase letter';
      if (control.errors?.['requiresDigit']) return 'Password must contain at least one digit';
    }
    return undefined;
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const { email, password, twoFactorCode } = this.loginForm.value;

    this.authService.login({ email, password, twoFactorCode }).subscribe({
      next: (response) => {
        if (response.requiresTwoFactor && !twoFactorCode) {
          this.show2FAInput = true;
          this.loading = false;
          this.toast.info('Please enter your 2FA code');
        } else {
          this.toast.success('Welcome back!');
          this.router.navigate(['/receipts']);
        }
      },
      error: (err) => {
        this.loading = false;
        
        // Handle validation errors (400 Bad Request with ModelState)
        if (err.status === 400 && err.error?.errors) {
          // Extract validation errors from ModelState
          const validationErrors = Object.values(err.error.errors).flat();
          const errorMessage = validationErrors.join('. ');
          this.toast.error(errorMessage);
        } else if (err.error?.message) {
          // Handle structured error messages
          this.toast.error(err.error.message);
        } else if (typeof err.error === 'string') {
          // Handle plain string errors
          this.toast.error(err.error);
        } else {
          // Fallback message
          this.toast.error('Login failed. Please try again.');
        }
      }
    });
  }
}
