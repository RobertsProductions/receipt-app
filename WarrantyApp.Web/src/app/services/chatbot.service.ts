import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ChatMessage,
  ChatbotResponse,
  SendMessageRequest,
  SuggestedQuestion,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class ChatbotService {
  private readonly apiUrl = `${environment.apiUrl}/Chatbot`;

  constructor(private http: HttpClient) {}

  sendMessage(message: string): Observable<ChatbotResponse> {
    const request: SendMessageRequest = { message };
    return this.http.post<ChatbotResponse>(`${this.apiUrl}/message`, request);
  }

  getHistory(
    limit: number = 50
  ): Observable<{ messages: ChatMessage[]; totalMessages: number }> {
    const params = new HttpParams()
      .set('limit', limit.toString());

    return this.http.get<{ messages: ChatMessage[]; totalMessages: number }>(
      `${this.apiUrl}/history`,
      { params }
    );
  }

  clearHistory(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/history`);
  }

  getSuggestedQuestions(): Observable<SuggestedQuestion[]> {
    return this.http.get<SuggestedQuestion[]>(`${this.apiUrl}/suggested-questions`);
  }
}
