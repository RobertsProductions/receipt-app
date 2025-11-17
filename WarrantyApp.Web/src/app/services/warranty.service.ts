import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ExpiringWarranty } from '../models';

@Injectable({
  providedIn: 'root',
})
export class WarrantyService {
  private readonly apiUrl = `${environment.apiUrl}/WarrantyNotifications`;

  constructor(private http: HttpClient) {}

  getExpiringWarranties(daysThreshold: number = 30): Observable<ExpiringWarranty[]> {
    const params = new HttpParams().set('daysThreshold', daysThreshold.toString());
    return this.http.get<ExpiringWarranty[]>(`${this.apiUrl}/expiring`, { params });
  }
}
