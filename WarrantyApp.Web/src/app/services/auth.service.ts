import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RefreshTokenRequest,
  TwoFactorSetupResponse,
  TwoFactorVerifyRequest,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/Auth`;
  private readonly accessTokenKey = 'access_token';
  private readonly refreshTokenKey = 'refresh_token';

  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    console.log('🌍 AuthService initialized with apiUrl:', environment.apiUrl);
    console.log('Full auth endpoint:', this.apiUrl);
    this.loadStoredAuth();
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, request).pipe(
      tap((response) => {
        if (!response.requiresTwoFactor) {
          this.storeAuth(response);
        }
      })
    );
  }

  loginWith2FA(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login/2fa`, request).pipe(
      tap((response) => this.storeAuth(response))
    );
  }

  register(request: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/register`, request).pipe(
      tap((response) => this.storeAuth(response))
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/logout`, {}).pipe(
      tap(() => this.clearAuth())
    );
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const request: RefreshTokenRequest = { refreshToken };
    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh`, request).pipe(
      tap((response) => this.storeAuth(response))
    );
  }

  confirmEmail(token: string): Observable<void> {
    return this.http.get<void>(`${this.apiUrl}/confirm-email`, {
      params: { token },
    });
  }

  resendConfirmationEmail(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/resend-confirmation-email`, {});
  }

  resendEmailConfirmation(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/resend-email-confirmation`, { email });
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reset-password`, { token, newPassword });
  }

  verifyPhone(code: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/verify-phone`, { code });
  }

  resendPhoneVerification(phoneNumber: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/resend-phone-verification`, { phoneNumber });
  }

  enable2FA(): Observable<TwoFactorSetupResponse> {
    return this.http.post<TwoFactorSetupResponse>(`${this.apiUrl}/2fa/enable`, {});
  }

  verify2FA(request: TwoFactorVerifyRequest): Observable<{ recoveryCodes: string[] }> {
    return this.http.post<{ recoveryCodes: string[] }>(
      `${this.apiUrl}/2fa/verify`,
      request
    );
  }

  disable2FA(code: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/2fa/disable`, { code });
  }

  get2FAStatus(): Observable<{ isEnabled: boolean }> {
    return this.http.get<{ isEnabled: boolean }>(`${this.apiUrl}/2fa/status`);
  }

  regenerateRecoveryCodes(): Observable<{ recoveryCodes: string[] }> {
    return this.http.post<{ recoveryCodes: string[] }>(
      `${this.apiUrl}/2fa/recovery-codes/regenerate`,
      {}
    );
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenKey);
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  getCurrentUser(): LoginResponse | null {
    return this.currentUserSubject.value;
  }

  private storeAuth(response: LoginResponse): void {
    console.log('🔐 Storing auth tokens...');
    console.log('Access token:', response.token ? `${response.token.substring(0, 20)}...` : 'MISSING');
    console.log('Refresh token:', response.refreshToken ? `${response.refreshToken.substring(0, 20)}...` : 'MISSING');
    
    localStorage.setItem(this.accessTokenKey, response.token);
    localStorage.setItem(this.refreshTokenKey, response.refreshToken);
    this.currentUserSubject.next(response);
    
    console.log('✅ Tokens stored. Verify in localStorage:');
    console.log('- access_token:', localStorage.getItem(this.accessTokenKey) ? 'EXISTS' : 'MISSING');
    console.log('- refresh_token:', localStorage.getItem(this.refreshTokenKey) ? 'EXISTS' : 'MISSING');
  }

  private clearAuth(): void {
    localStorage.removeItem(this.accessTokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    this.currentUserSubject.next(null);
  }

  private loadStoredAuth(): void {
    const token = this.getAccessToken();
    if (token) {
      // Could decode JWT here to get user info if needed
      // For now, just mark as authenticated
    }
  }
}
