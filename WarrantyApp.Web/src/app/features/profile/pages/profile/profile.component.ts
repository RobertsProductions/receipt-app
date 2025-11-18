import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { ModalComponent } from '../../../../shared/components/modal/modal.component';
import { UserProfileService } from '../../../../services/user-profile.service';
import { AuthService } from '../../../../services/auth.service';
import { ReceiptService } from '../../../../services/receipt.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { UserProfile } from '../../../../models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ButtonComponent,
    InputComponent,
    CardComponent,
    AvatarComponent,
    BadgeComponent,
    SpinnerComponent,
    ModalComponent
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  loading: boolean = false;
  editMode: boolean = false;
  showPasswordModal: boolean = false;
  resendingEmail: boolean = false;
  profile: UserProfile | null = null;
  profileForm: FormGroup;
  passwordForm: FormGroup;
  stats = { totalReceipts: 0, warrantiesTracked: 0 };
  savingProfile: boolean = false;
  changingPassword: boolean = false;

  constructor(
    private fb: FormBuilder,
    private userProfileService: UserProfileService,
    private authService: AuthService,
    private receiptService: ReceiptService,
    private toast: ToastService,
    private router: Router
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['', [Validators.pattern(/^\+?[1-9]\d{1,14}$/)]]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    this.loadProfile();
    this.loadStats();
  }

  loadProfile(): void {
    this.loading = true;
    this.userProfileService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        this.profileForm.patchValue({
          firstName: profile.firstName,
          lastName: profile.lastName,
          phoneNumber: profile.phoneNumber
        });
        this.loading = false;
      },
      error: () => {
        this.toast.error('Failed to load profile');
        this.loading = false;
      }
    });
  }

  loadStats(): void {
    this.receiptService.getReceipts(1, 1).subscribe({
      next: (response) => {
        this.stats.totalReceipts = response.totalCount;
        this.stats.warrantiesTracked = response.receipts.filter(r => r.warrantyExpirationDate).length;
      }
    });
  }

  toggleEditMode(): void {
    if (this.editMode) {
      this.profileForm.patchValue({
        firstName: this.profile?.firstName,
        lastName: this.profile?.lastName,
        phoneNumber: this.profile?.phoneNumber
      });
    }
    this.editMode = !this.editMode;
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.toast.error('Please fill all required fields');
      return;
    }

    this.savingProfile = true;
    this.userProfileService.updateProfile(this.profileForm.value).subscribe({
      next: (profile) => {
        this.profile = profile;
        this.editMode = false;
        this.savingProfile = false;
        this.toast.success('Profile updated successfully');
      },
      error: () => {
        this.savingProfile = false;
        this.toast.error('Failed to update profile');
      }
    });
  }

  openPasswordModal(): void {
    this.passwordForm.reset();
    this.showPasswordModal = true;
  }

  closePasswordModal(): void {
    this.showPasswordModal = false;
    this.passwordForm.reset();
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.toast.error('Please fill all fields correctly');
      return;
    }

    this.changingPassword = true;
    const { currentPassword, newPassword } = this.passwordForm.value;

    this.userProfileService.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.changingPassword = false;
        this.closePasswordModal();
        this.toast.success('Password changed successfully');
      },
      error: (error) => {
        this.changingPassword = false;
        this.toast.error(error.error?.message || 'Failed to change password');
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getInitials(): string {
    if (!this.profile) return '?';
    const first = this.profile.firstName?.charAt(0) || '';
    const last = this.profile.lastName?.charAt(0) || '';
    return (first + last).toUpperCase() || '?';
  }

  getMemberSince(): string {
    if (!this.profile?.createdAt) return 'Unknown';
    const date = new Date(this.profile.createdAt);
    return date.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  resendConfirmation(): void {
    if (!this.profile?.email) {
      this.toast.error('Email address not found');
      return;
    }

    this.resendingEmail = true;
    this.authService.resendEmailConfirmation(this.profile.email).subscribe({
      next: () => {
        this.toast.success('Verification email sent! Please check your inbox.');
        this.resendingEmail = false;
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Failed to send verification email');
        this.resendingEmail = false;
      }
    });
  }
}
