import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { AuthService } from '../../../../services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, ButtonComponent, InputComponent, CardComponent],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  email = '';
  loading = false;
  emailSent = false;

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  async sendResetLink() {
    if (!this.email) {
      this.toastService.error('Please enter your email address');
      return;
    }

    this.loading = true;

    try {
      await this.authService.forgotPassword(this.email).toPromise();
      this.emailSent = true;
      this.toastService.success('Password reset link sent to your email');
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to send reset link');
    } finally {
      this.loading = false;
    }
  }

  backToLogin() {
    this.router.navigate(['/login']);
  }
}
