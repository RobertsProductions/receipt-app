import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ReceiptShare, ShareReceiptRequest, SharedReceipt } from '../models';

@Injectable({
  providedIn: 'root',
})
export class SharingService {
  private readonly apiUrl = `${environment.apiUrl}/ReceiptSharing`;

  constructor(private http: HttpClient) {}

  shareReceipt(request: ShareReceiptRequest): Observable<ReceiptShare> {
    return this.http.post<ReceiptShare>(`${this.apiUrl}/share`, request);
  }

  getSharedWithMe(): Observable<SharedReceipt[]> {
    return this.http.get<SharedReceipt[]>(`${this.apiUrl}/shared-with-me`);
  }

  getMyShares(): Observable<ReceiptShare[]> {
    return this.http.get<ReceiptShare[]>(`${this.apiUrl}/my-shares`);
  }

  getReceiptShares(receiptId: string): Observable<ReceiptShare[]> {
    return this.http.get<ReceiptShare[]>(`${this.apiUrl}/${receiptId}/shares`);
  }

  revokeShare(shareId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${shareId}`);
  }
}
