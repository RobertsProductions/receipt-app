import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { AuthService } from '../../../../services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

type ConfirmationState = 'loading' | 'success' | 'error';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule, ButtonComponent, CardComponent],
  templateUrl: './confirm-email.component.html',
  styleUrl: './confirm-email.component.scss'
})
export class ConfirmEmailComponent implements OnInit {
  state: ConfirmationState = 'loading';
  errorMessage = '';
  resendLoading = false;
  email = '';

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  async ngOnInit() {
    const token = this.route.snapshot.queryParams['token'];
    this.email = this.route.snapshot.queryParams['email'] || '';

    // If no token, user probably navigated here directly - not an error
    if (!token) {
      this.state = 'error';
      this.errorMessage = 'No confirmation token provided. If you just registered, please check your email for the confirmation link.';
      return;
    }

    try {
      await this.authService.confirmEmail(token).toPromise();
      this.state = 'success';
      this.toastService.success('Email confirmed successfully!');
    } catch (error: any) {
      this.state = 'error';
      this.errorMessage = error.error?.message || 'Email confirmation failed. The link may have expired.';
    }
  }

  async resendConfirmation() {
    if (!this.email) {
      this.toastService.error('Email address not found');
      return;
    }

    this.resendLoading = true;

    try {
      await this.authService.resendEmailConfirmation(this.email).toPromise();
      this.toastService.success('Confirmation email sent!');
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to resend confirmation');
    } finally {
      this.resendLoading = false;
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

  goToHome() {
    this.router.navigate(['/']);
  }
}
