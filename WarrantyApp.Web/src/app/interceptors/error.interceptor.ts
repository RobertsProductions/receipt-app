import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Only trigger logout and redirect if this is not a login/auth endpoint
        // to avoid cascading logout calls on failed login attempts
        const isAuthEndpoint = req.url.includes('/Auth/login') || 
                               req.url.includes('/Auth/register') ||
                               req.url.includes('/Auth/refresh');
        
        if (!isAuthEndpoint && authService.isAuthenticated()) {
          // Clear local auth state without making API call to avoid loops
          localStorage.removeItem('access_token');
          localStorage.removeItem('refresh_token');
          router.navigate(['/login']);
        }
      }

      // Log error to console in development
      if (typeof window !== 'undefined' && !window.location.hostname.includes('production')) {
        console.error('HTTP Error:', error);
      }

      return throwError(() => error);
    })
  );
};
