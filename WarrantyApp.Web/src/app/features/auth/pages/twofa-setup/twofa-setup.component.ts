import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { ModalComponent } from '../../../../shared/components/modal/modal.component';
import { AuthService } from '../../../../services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';

type SetupStep = 'qrcode' | 'verify' | 'recoveryCodes' | 'complete';

@Component({
  selector: 'app-twofa-setup',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent, InputComponent, CardComponent, ModalComponent],
  templateUrl: './twofa-setup.component.html',
  styleUrl: './twofa-setup.component.scss'
})
export class TwofaSetupComponent implements OnInit {
  currentStep: SetupStep = 'qrcode';
  loading = true;
  qrCodeDataUrl = '';
  manualEntryKey = '';
  verificationCode = '';
  recoveryCodes: string[] = [];
  verifyLoading = false;
  downloaded = false;

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  async ngOnInit() {
    await this.initiate2FASetup();
  }

  async initiate2FASetup() {
    this.loading = true;

    try {
      const response = await this.authService.enable2FA().toPromise();
      this.qrCodeDataUrl = response!.qrCodeUrl;
      this.manualEntryKey = response!.sharedKey;
      this.currentStep = 'qrcode';
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Failed to setup 2FA');
      this.router.navigate(['/profile']);
    } finally {
      this.loading = false;
    }
  }

  proceedToVerify() {
    if (!this.verificationCode || this.verificationCode.length !== 6) {
      this.toastService.error('Please enter a 6-digit code');
      return;
    }

    this.currentStep = 'verify';
    this.verify2FA();
  }

  async verify2FA() {
    this.verifyLoading = true;

    try {
      const response = await this.authService.verify2FA({ code: this.verificationCode }).toPromise();
      this.recoveryCodes = response!.recoveryCodes;
      this.currentStep = 'recoveryCodes';
      this.toastService.success('2FA enabled successfully!');
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Invalid verification code');
      this.currentStep = 'qrcode';
      this.verificationCode = '';
    } finally {
      this.verifyLoading = false;
    }
  }

  downloadRecoveryCodes() {
    const content = this.recoveryCodes.join('\n');
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'recovery-codes.txt';
    link.click();
    window.URL.revokeObjectURL(url);
    this.downloaded = true;
  }

  copyRecoveryCodes() {
    const content = this.recoveryCodes.join('\n');
    navigator.clipboard.writeText(content).then(() => {
      this.toastService.success('Recovery codes copied to clipboard');
    });
  }

  complete() {
    if (!this.downloaded) {
      this.toastService.warning('Please download your recovery codes before completing');
      return;
    }

    this.currentStep = 'complete';
    setTimeout(() => {
      this.router.navigate(['/profile']);
    }, 2000);
  }

  cancel() {
    this.router.navigate(['/profile']);
  }
}
