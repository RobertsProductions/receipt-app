export interface User {
  id: string;
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
  createdAt?: string;
}

export interface UserProfile {
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  notificationPreference: 'Email' | 'Sms' | 'Both' | 'None';
  warrantyExpirationThresholdDays: number;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
  createdAt?: string;
}
