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
        // Unauthorized - token expired or invalid
        authService.logout().subscribe(() => {
          router.navigate(['/login']);
        });
      }

      // Log error to console in development
      if (typeof window !== 'undefined' && !window.location.hostname.includes('production')) {
        console.error('HTTP Error:', error);
      }

      return throwError(() => error);
    })
  );
};
