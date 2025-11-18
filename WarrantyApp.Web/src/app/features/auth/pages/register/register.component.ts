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
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    ButtonComponent,
    InputComponent,
    CardComponent
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toast: ToastService
  ) {
    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, this.passwordValidator.bind(this)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
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

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (!password || !confirmPassword) {
      return null;
    }

    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  get usernameError(): string | undefined {
    const control = this.registerForm.get('username');
    if (control?.touched && control?.invalid) {
      if (control.errors?.['required']) return 'Username is required';
      if (control.errors?.['minlength']) return 'Username must be at least 3 characters';
    }
    return undefined;
  }

  get emailError(): string | undefined {
    const control = this.registerForm.get('email');
    if (control?.touched && control?.invalid) {
      if (control.errors?.['required']) return 'Email is required';
      if (control.errors?.['email']) return 'Invalid email format';
    }
    return undefined;
  }

  get passwordError(): string | undefined {
    const control = this.registerForm.get('password');
    if (control?.touched && control?.invalid) {
      if (control.errors?.['required']) return 'Password is required';
      if (control.errors?.['minlength']) return 'Password must be at least 6 characters';
      if (control.errors?.['requiresUppercase']) return 'Password must contain at least one uppercase letter';
      if (control.errors?.['requiresLowercase']) return 'Password must contain at least one lowercase letter';
      if (control.errors?.['requiresDigit']) return 'Password must contain at least one digit';
    }
    return undefined;
  }

  get confirmPasswordError(): string | undefined {
    const control = this.registerForm.get('confirmPassword');
    if (control?.touched && control?.invalid) {
      if (control.errors?.['required']) return 'Please confirm your password';
    }
    if (this.registerForm.errors?.['passwordMismatch'] && control?.touched) {
      return 'Passwords do not match';
    }
    return undefined;
  }

  get passwordStrength(): 'weak' | 'medium' | 'strong' {
    const password = this.registerForm.get('password')?.value || '';
    
    if (password.length < 6) return 'weak';
    
    const hasLower = /[a-z]/.test(password);
    const hasUpper = /[A-Z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSymbol = /[^a-zA-Z0-9]/.test(password);
    
    const strength = [hasLower, hasUpper, hasNumber, hasSymbol].filter(Boolean).length;
    
    if (strength >= 3 && password.length >= 6) return 'strong';
    if (strength >= 2 && password.length >= 6) return 'medium';
    return 'weak';
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const { username, email, password } = this.registerForm.value;

    this.authService.register({ userName: username, email, password }).subscribe({
      next: () => {
        this.toast.success('Account created successfully! You can verify your email later from your profile.');
        this.router.navigate(['/receipts']);
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
          this.toast.error('Registration failed. Please try again.');
        }
      }
    });
  }
}
