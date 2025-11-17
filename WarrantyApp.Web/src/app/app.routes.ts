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
    path: 'receipts',
    canActivate: [authGuard],
    loadComponent: () => import('./features/receipts/pages/receipt-list/receipt-list.component').then(m => m.ReceiptListComponent)
  },
  {
    path: 'receipts/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/receipts/pages/receipt-detail/receipt-detail.component').then(m => m.ReceiptDetailComponent)
  },
  {
    path: '**',
    redirectTo: ''
  }
];
