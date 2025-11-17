import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getAccessToken();

  console.log('🔒 Auth Interceptor:', req.url);
  console.log('Token exists:', !!token);
  if (token) {
    console.log('Token preview:', token.substring(0, 30) + '...');
  }

  if (token) {
    const clonedReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
    console.log('✅ Added Authorization header');
    console.log('Verify header in cloned request:', clonedReq.headers.get('Authorization'));
    return next(clonedReq);
  }

  console.log('⚠️ No token - request sent without Authorization header');
  return next(req);
};
