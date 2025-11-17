export interface ReceiptShare {
  id: string;
  receiptId: string;
  ownerUserId: string;
  recipientUserId: string;
  sharedDate: string;
  ownerUserName?: string;
  recipientUserName?: string;
}

export interface ShareReceiptRequest {
  receiptId: string;
  recipientIdentifier: string;
}

export interface SharedReceipt {
  share: ReceiptShare;
  receipt: {
    id: string;
    merchant?: string;
    amount?: number;
    purchaseDate?: string;
    productName?: string;
    warrantyExpirationDate?: string;
    fileName: string;
  };
}
