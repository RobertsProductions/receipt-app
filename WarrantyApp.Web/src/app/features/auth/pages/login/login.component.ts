import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
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
      password: ['', [Validators.required, Validators.minLength(6)]],
      twoFactorCode: ['']
    });
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
        const errorMessage = err.error?.message || 'Login failed. Please try again.';
        this.toast.error(errorMessage);
      }
    });
  }
}
