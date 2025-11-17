import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserProfile } from '../models';

@Injectable({
  providedIn: 'root',
})
export class UserProfileService {
  private readonly apiUrl = `${environment.apiUrl}/UserProfile`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(this.apiUrl);
  }

  updateProfile(profile: Partial<UserProfile>): Observable<UserProfile> {
    return this.http.put<UserProfile>(this.apiUrl, profile);
  }

  changePassword(
    currentPassword: string,
    newPassword: string
  ): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/change-password`, {
      currentPassword,
      newPassword,
    });
  }

  verifyPhone(phoneNumber: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/verify-phone`, {
      phoneNumber,
    });
  }

  confirmPhone(code: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/confirm-phone`, {
      code,
    });
  }
}
