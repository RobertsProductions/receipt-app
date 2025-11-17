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
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly accessTokenKey = 'access_token';
  private readonly refreshTokenKey = 'refresh_token';

  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
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
    localStorage.setItem(this.accessTokenKey, response.accessToken);
    localStorage.setItem(this.refreshTokenKey, response.refreshToken);
    this.currentUserSubject.next(response);
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
