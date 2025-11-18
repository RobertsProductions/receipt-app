import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Receipt,
  ReceiptUpdateRequest,
  OcrResult,
  BatchOcrRequest,
  BatchOcrResult,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class ReceiptService {
  private readonly apiUrl = `${environment.apiUrl}/Receipts`;

  constructor(private http: HttpClient) {}

  getReceipts(
    pageNumber: number = 1,
    pageSize: number = 20
  ): Observable<{ receipts: Receipt[]; totalCount: number }> {
    const params = new HttpParams()
      .set('page', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<{ receipts: Receipt[]; totalCount: number }>(this.apiUrl, {
      params,
    });
  }

  getReceipt(id: string): Observable<Receipt> {
    return this.http.get<Receipt>(`${this.apiUrl}/${id}`);
  }

  uploadReceipt(
    file: File,
    useOcr: boolean = false,
    metadata?: Partial<ReceiptUpdateRequest>
  ): Observable<Receipt> {
    const formData = new FormData();
    formData.append('file', file);

    if (metadata) {
      if (metadata.merchant) formData.append('Merchant', metadata.merchant);
      if (metadata.amount) formData.append('Amount', metadata.amount.toString());
      if (metadata.purchaseDate) formData.append('PurchaseDate', metadata.purchaseDate);
      if (metadata.productName) formData.append('ProductName', metadata.productName);
      if (metadata.warrantyMonths)
        formData.append('WarrantyMonths', metadata.warrantyMonths.toString());
      if (metadata.notes) formData.append('Notes', metadata.notes);
    }

    const params = new HttpParams().set('useOcr', useOcr.toString());

    return this.http.post<Receipt>(`${this.apiUrl}/upload`, formData, { params });
  }

  updateReceipt(id: string, request: ReceiptUpdateRequest): Observable<Receipt> {
    return this.http.put<Receipt>(`${this.apiUrl}/${id}`, request);
  }

  deleteReceipt(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadReceipt(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/download`, {
      responseType: 'blob',
    });
  }

  processOcr(id: string): Observable<OcrResult> {
    return this.http.post<OcrResult>(`${this.apiUrl}/${id}/ocr`, {});
  }

  batchOcr(request: BatchOcrRequest): Observable<BatchOcrResult> {
    return this.http.post<BatchOcrResult>(`${this.apiUrl}/batch-ocr`, request);
  }

  getSharedWithMe(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/shared-with-me`);
  }

  getReceiptShares(receiptId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${receiptId}/shares`);
  }

  shareReceipt(receiptId: number, email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${receiptId}/share`, { email });
  }

  revokeShare(receiptId: number, email: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${receiptId}/share/${encodeURIComponent(email)}`);
  }
}
