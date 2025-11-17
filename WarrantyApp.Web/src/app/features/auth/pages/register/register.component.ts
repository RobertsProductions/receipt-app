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
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
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
      if (control.errors?.['minlength']) return 'Password must be at least 8 characters';
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
    
    if (password.length < 8) return 'weak';
    
    const hasLower = /[a-z]/.test(password);
    const hasUpper = /[A-Z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSymbol = /[^a-zA-Z0-9]/.test(password);
    
    const strength = [hasLower, hasUpper, hasNumber, hasSymbol].filter(Boolean).length;
    
    if (strength >= 3 && password.length >= 8) return 'strong';
    if (strength >= 2 && password.length >= 8) return 'medium';
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
        this.toast.success('Account created! Please check your email to confirm.');
        this.router.navigate(['/confirm-email'], { queryParams: { email } });
      },
      error: (err) => {
        this.loading = false;
        const errorMessage = err.error?.message || 'Registration failed. Please try again.';
        this.toast.error(errorMessage);
      }
    });
  }
}
