import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface CanvasAction {
  id: string;
  type: string;
  productId: number;
  label: string;
  x: number;
  y: number;
  width: number;
  height: number;
  imageURL?: string | null;
  price?: number | null;
  color?: string | null;
}

export interface ChatResponse {
  reply: string;
  canvasActions?: CanvasAction[] | null;
  floorImageURL?: string | null;
  wallImageURL?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  constructor(private http: HttpClient) {}

  send(message: string, history: ChatMessage[]) {
    return this.http.post<ChatResponse>('/api/chat', {
      message,
      history
    });
  }

  search(query: string) {
    return this.http.post<{ results: any[] }>('/api/search', { query });
  }
}
