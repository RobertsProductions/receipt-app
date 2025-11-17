import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/auth/pages/landing/landing.component').then(m => m.LandingComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/pages/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/pages/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: 'confirm-email',
    loadComponent: () => import('./features/auth/pages/confirm-email/confirm-email.component').then(m => m.ConfirmEmailComponent)
  },
  {
    path: 'verify-phone',
    canActivate: [authGuard],
    loadComponent: () => import('./features/auth/pages/verify-phone/verify-phone.component').then(m => m.VerifyPhoneComponent)
  },
  {
    path: '2fa/setup',
    canActivate: [authGuard],
    loadComponent: () => import('./features/auth/pages/twofa-setup/twofa-setup.component').then(m => m.TwofaSetupComponent)
  },
  {
    path: 'receipts',
    canActivate: [authGuard],
    loadComponent: () => import('./features/receipts/pages/receipt-list/receipt-list.component').then(m => m.ReceiptListComponent)
  },
  {
    path: 'receipts/shared',
    canActivate: [authGuard],
    loadComponent: () => import('./features/sharing/pages/shared-receipts/shared-receipts.component').then(m => m.SharedReceiptsComponent)
  },
  {
    path: 'receipts/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/receipts/pages/receipt-detail/receipt-detail.component').then(m => m.ReceiptDetailComponent)
  },
  {
    path: 'warranties',
    canActivate: [authGuard],
    loadComponent: () => import('./features/warranties/pages/warranty-dashboard/warranty-dashboard.component').then(m => m.WarrantyDashboardComponent)
  },
  {
    path: 'profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/profile/pages/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'settings/notifications',
    canActivate: [authGuard],
    loadComponent: () => import('./features/settings/pages/notification-settings/notification-settings.component').then(m => m.NotificationSettingsComponent)
  },
  {
    path: 'chatbot',
    canActivate: [authGuard],
    loadComponent: () => import('./features/chatbot/pages/chatbot/chatbot.component').then(m => m.ChatbotComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
