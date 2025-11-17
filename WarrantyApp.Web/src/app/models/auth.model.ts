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
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  userName: string;
  email: string;
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
