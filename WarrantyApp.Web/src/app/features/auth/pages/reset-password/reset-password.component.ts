import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { AuthService } from '../../../../services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, InputComponent, CardComponent],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  token = '';
  email = '';
  password = '';
  confirmPassword = '';
  loading = false;
  resetSuccess = false;

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.token = this.route.snapshot.queryParams['token'] || '';
    this.email = this.route.snapshot.queryParams['email'] || '';

    if (!this.token) {
      this.toastService.error('Invalid reset link');
      this.router.navigate(['/forgot-password']);
    }
  }

  get passwordsMatch(): boolean {
    return this.password === this.confirmPassword;
  }

  get isValid(): boolean {
    return this.password.length >= 8 && this.passwordsMatch;
  }

  async resetPassword() {
    if (!this.isValid) {
      this.toastService.error('Please check your password entries');
      return;
    }

    this.loading = true;

    try {
      await this.authService.resetPassword(this.token, this.password).toPromise();
      this.resetSuccess = true;
      this.toastService.success('Password reset successfully!');
      
      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 2000);
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to reset password');
    } finally {
      this.loading = false;
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
