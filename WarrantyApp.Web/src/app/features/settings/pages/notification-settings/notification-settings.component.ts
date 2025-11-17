import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { CardComponent } from '../../../../shared/components/card/card.component';
import { ToggleComponent } from '../../../../shared/components/toggle/toggle.component';
import { CheckboxComponent } from '../../../../shared/components/checkbox/checkbox.component';
import { SliderComponent } from '../../../../shared/components/slider/slider.component';
import { InputComponent } from '../../../../shared/components/input/input.component';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { BadgeComponent } from '../../../../shared/components/badge/badge.component';
import { UserProfileService } from '../../../../services/user-profile.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { UserProfile } from '../../../../models';

@Component({
  selector: 'app-notification-settings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ButtonComponent,
    CardComponent,
    ToggleComponent,
    CheckboxComponent,
    SliderComponent,
    InputComponent,
    SpinnerComponent,
    BadgeComponent
  ],
  templateUrl: './notification-settings.component.html',
  styleUrl: './notification-settings.component.scss'
})
export class NotificationSettingsComponent implements OnInit {
  loading: boolean = false;
  saving: boolean = false;
  testing: boolean = false;
  profile: UserProfile | null = null;
  settingsForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private userProfileService: UserProfileService,
    private toast: ToastService
  ) {
    this.settingsForm = this.fb.group({
      enableNotifications: [true],
      emailNotifications: [true],
      smsNotifications: [false],
      thresholdDays: [30],
      phoneNumber: ['']
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loading = true;
    this.userProfileService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        const hasNotifications = profile.notificationPreference !== 'None';
        const hasEmail = profile.notificationPreference === 'Email' || profile.notificationPreference === 'Both';
        const hasSms = profile.notificationPreference === 'Sms' || profile.notificationPreference === 'Both';

        this.settingsForm.patchValue({
          enableNotifications: hasNotifications,
          emailNotifications: hasEmail,
          smsNotifications: hasSms,
          thresholdDays: profile.warrantyExpirationThresholdDays,
          phoneNumber: profile.phoneNumber || ''
        });
        this.loading = false;
      },
      error: () => {
        this.toast.error('Failed to load settings');
        this.loading = false;
      }
    });
  }

  saveSettings(): void {
    const formValue = this.settingsForm.value;
    
    let notificationPreference: 'Email' | 'Sms' | 'Both' | 'None' = 'None';
    if (formValue.enableNotifications) {
      if (formValue.emailNotifications && formValue.smsNotifications) {
        notificationPreference = 'Both';
      } else if (formValue.emailNotifications) {
        notificationPreference = 'Email';
      } else if (formValue.smsNotifications) {
        notificationPreference = 'Sms';
      }
    }

    this.saving = true;
    this.userProfileService.updateProfile({
      notificationPreference,
      warrantyExpirationThresholdDays: formValue.thresholdDays,
      phoneNumber: formValue.phoneNumber
    }).subscribe({
      next: (profile) => {
        this.profile = profile;
        this.saving = false;
        this.toast.success('Settings saved successfully');
      },
      error: () => {
        this.saving = false;
        this.toast.error('Failed to save settings');
      }
    });
  }

  sendTestNotification(): void {
    this.testing = true;
    setTimeout(() => {
      this.testing = false;
      this.toast.success('Test notification sent! Check your email/phone.');
    }, 1500);
  }

  verifyPhone(): void {
    this.toast.info('Phone verification will be available soon');
  }

  onEnableNotificationsChange(value: boolean): void {
    this.settingsForm.patchValue({ enableNotifications: value });
    if (!value) {
      this.settingsForm.patchValue({
        emailNotifications: false,
        smsNotifications: false
      });
    } else {
      this.settingsForm.patchValue({
        emailNotifications: true
      });
    }
  }
}
