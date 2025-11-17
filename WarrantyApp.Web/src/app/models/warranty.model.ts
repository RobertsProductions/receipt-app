export interface ExpiringWarranty {
  receiptId: string;
  merchant: string;
  productName: string;
  purchaseDate: string;
  warrantyExpirationDate: string;
  daysUntilExpiration: number;
  amount?: number;
}

export interface WarrantyNotificationSettings {
  enabled: boolean;
  thresholdDays: number;
  channels: ('Email' | 'Sms')[];
}
