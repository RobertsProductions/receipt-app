export interface LoginRequest {
  email: string;
  password: string;
  twoFactorCode?: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
}

export interface LoginResponse {
  token: string;           // Backend returns 'Token' (lowercase in JSON)
  refreshToken: string;
  expiresAt: string;       // Backend returns 'ExpiresAt' 
  email: string;
  username: string;        // Backend returns 'Username'
  requiresTwoFactor?: boolean;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface TwoFactorSetupResponse {
  sharedKey: string;
  qrCodeUrl: string;
  recoveryCodes: string[];
}

export interface TwoFactorVerifyRequest {
  code: string;
}
