import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  constructor(private http: HttpClient) {}

  send(message: string, history: ChatMessage[]) {
    return this.http.post<{ reply: string }>('/api/chat', {
      message,
      history
    });
  }

  search(query: string) {
    return this.http.post<{ results: any[] }>('/api/search', { query });
  }
}
