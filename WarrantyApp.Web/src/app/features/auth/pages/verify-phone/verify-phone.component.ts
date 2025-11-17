import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { AuthService } from '../../../../services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-verify-phone',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, CardComponent],
  templateUrl: './verify-phone.component.html',
  styleUrl: './verify-phone.component.scss'
})
export class VerifyPhoneComponent implements OnInit {
  code: string[] = ['', '', '', '', '', ''];
  phoneNumber: string = '';
  loading = false;
  resendLoading = false;
  resendCooldown = 0;

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.phoneNumber = this.route.snapshot.queryParams['phone'] || '';
    if (!this.phoneNumber) {
      this.router.navigate(['/profile']);
    }
  }

  onCodeInput(index: number, event: Event) {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    if (value && /^\d$/.test(value)) {
      this.code[index] = value;
      
      // Move to next input
      if (index < 5) {
        const nextInput = document.getElementById(`code-${index + 1}`) as HTMLInputElement;
        nextInput?.focus();
      }
    } else {
      this.code[index] = '';
    }
  }

  onKeyDown(index: number, event: KeyboardEvent) {
    if (event.key === 'Backspace' && !this.code[index] && index > 0) {
      const prevInput = document.getElementById(`code-${index - 1}`) as HTMLInputElement;
      prevInput?.focus();
    } else if (event.key === 'ArrowLeft' && index > 0) {
      const prevInput = document.getElementById(`code-${index - 1}`) as HTMLInputElement;
      prevInput?.focus();
    } else if (event.key === 'ArrowRight' && index < 5) {
      const nextInput = document.getElementById(`code-${index + 1}`) as HTMLInputElement;
      nextInput?.focus();
    }
  }

  onPaste(event: ClipboardEvent) {
    event.preventDefault();
    const pastedData = event.clipboardData?.getData('text') || '';
    const digits = pastedData.replace(/\D/g, '').slice(0, 6);
    
    for (let i = 0; i < digits.length; i++) {
      this.code[i] = digits[i];
    }

    const lastIndex = Math.min(digits.length, 5);
    const lastInput = document.getElementById(`code-${lastIndex}`) as HTMLInputElement;
    lastInput?.focus();
  }

  get isCodeComplete(): boolean {
    return this.code.every(digit => digit !== '');
  }

  async verifyCode() {
    if (!this.isCodeComplete) return;

    this.loading = true;
    const verificationCode = this.code.join('');

    try {
      await this.authService.verifyPhone(verificationCode).toPromise();
      this.toastService.success('Phone number verified successfully!');
      this.router.navigate(['/profile']);
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Invalid verification code');
      this.code = ['', '', '', '', '', ''];
      const firstInput = document.getElementById('code-0') as HTMLInputElement;
      firstInput?.focus();
    } finally {
      this.loading = false;
    }
  }

  async resendCode() {
    if (this.resendCooldown > 0) return;

    this.resendLoading = true;

    try {
      await this.authService.resendPhoneVerification(this.phoneNumber).toPromise();
      this.toastService.success('Verification code sent!');
      
      // Start cooldown
      this.resendCooldown = 60;
      const interval = setInterval(() => {
        this.resendCooldown--;
        if (this.resendCooldown === 0) {
          clearInterval(interval);
        }
      }, 1000);
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to resend code');
    } finally {
      this.resendLoading = false;
    }
  }

  cancel() {
    this.router.navigate(['/profile']);
  }
}
