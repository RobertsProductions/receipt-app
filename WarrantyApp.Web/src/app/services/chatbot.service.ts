import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  ChatMessage,
  ChatMessageResponseDto,
  SendMessageRequest,
  SuggestedQuestion,
} from '../models';

@Injectable({
  providedIn: 'root',
})
export class ChatbotService {
  private readonly apiUrl = `${environment.apiUrl}/Chatbot`;

  constructor(private http: HttpClient) {}

  sendMessage(message: string): Observable<ChatMessageResponseDto> {
    const request: SendMessageRequest = { message };
    return this.http.post<ChatMessageResponseDto>(`${this.apiUrl}/message`, request);
  }

  getHistory(
    limit: number = 50
  ): Observable<{ messages: ChatMessage[]; totalMessages: number }> {
    const params = new HttpParams()
      .set('limit', limit.toString());

    return this.http.get<{ messages: ChatMessageResponseDto[]; totalMessages: number }>(
      `${this.apiUrl}/history`,
      { params }
    ).pipe(
      map(response => ({
        messages: response.messages.map(msg => ({
          id: msg.messageId,
          role: msg.role as 'user' | 'assistant',
          content: msg.content,
          timestamp: msg.createdAt
        })),
        totalMessages: response.totalMessages
      }))
    );
  }

  clearHistory(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/history`);
  }

  getSuggestedQuestions(): Observable<SuggestedQuestion[]> {
    return this.http.get<SuggestedQuestion[]>(`${this.apiUrl}/suggested-questions`);
  }
}
