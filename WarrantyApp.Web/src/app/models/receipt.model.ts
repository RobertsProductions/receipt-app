export interface Receipt {
  id: string;
  userId: string;
  fileName: string;
  filePath: string;
  uploadDate: string;
  merchant?: string;
  amount?: number;
  purchaseDate?: string;
  productName?: string;
  warrantyMonths?: number;
  warrantyExpirationDate?: string;
  notes?: string;
  ocrProcessed: boolean;
}

export interface ReceiptUploadRequest {
  file: File;
  useOcr?: boolean;
  merchant?: string;
  amount?: number;
  purchaseDate?: string;
  productName?: string;
  warrantyMonths?: number;
  notes?: string;
}

export interface ReceiptUpdateRequest {
  merchant?: string;
  amount?: number;
  purchaseDate?: string;
  productName?: string;
  warrantyMonths?: number;
  notes?: string;
}

export interface OcrResult {
  merchant?: string;
  amount?: number;
  purchaseDate?: string;
  productName?: string;
  confidence?: number;
  rawText?: string;
}

export interface BatchOcrRequest {
  receiptIds: string[];
}

export interface BatchOcrResult {
  totalProcessed: number;
  successfulOcr: number;
  failedOcr: number;
  results: Array<{
    receiptId: string;
    success: boolean;
    error?: string;
    ocrData?: OcrResult;
  }>;
}
