import { Component, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService, ChatMessage } from './chat.service';

// Emotion → emoji mapping (face-api.js emotion labels)
const EMOTION_EMOJI: Record<string, string> = {
  happy:     '😊',
  sad:       '😢',
  angry:     '😠',
  surprised: '😲',
  fearful:   '😨',
  disgusted: '🤢',
  neutral:   '😐',
};

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnDestroy {
  @ViewChild('videoEl') videoEl!: ElementRef<HTMLVideoElement>;

  messages: ChatMessage[] = [];
  input = '';
  loading = false;
  isOpen = false;

  // Camera state
  cameraActive = false;
  cameraError = '';
  loadingModels = false;
  private stream: MediaStream | null = null;
  private faceApiLoaded = false;

  constructor(private chatService: ChatService) {}

  // ── Chat ──────────────────────────────────────────────────

  send() {
    if (!this.input.trim() || this.loading) return;

    const msg = this.input.trim();
    this.input = '';
    this.loading = true;

    this.messages.push({ role: 'user', content: msg });

    this.chatService.send(msg, this.messages.slice(0, -1)).subscribe({
      next: res => {
        this.messages.push({ role: 'assistant', content: res.reply });
        this.loading = false;
      },
      error: () => {
        this.messages.push({ role: 'assistant', content: 'Sorry, something went wrong. Please try again.' });
        this.loading = false;
      }
    });
  }

  onKey(e: KeyboardEvent) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      this.send();
    }
  }

  toggle() {
    this.isOpen = !this.isOpen;
    if (!this.isOpen) this.stopCamera();
  }

  // ── Camera & face-api.js ──────────────────────────────────

  async onCameraClick() {
    this.cameraError = '';
    this.cameraActive = true;

    // Load face-api.js models on first use
    if (!this.faceApiLoaded) {
      this.loadingModels = true;
      try {
        await this.loadFaceApi();
      } catch {
        this.cameraError = 'Failed to load face detection models.';
        this.cameraActive = false;
        this.loadingModels = false;
        return;
      }
      this.loadingModels = false;
    }

    // Request camera permission — SECURITY: only on explicit user action
    try {
      this.stream = await navigator.mediaDevices.getUserMedia({ video: true });
    } catch {
      this.cameraError = 'Camera access denied.';
      this.cameraActive = false;
      return;
    }

    // Wait for view to render the <video> element
    setTimeout(() => this.startDetection(), 100);
  }

  private async loadFaceApi() {
    // face-api.js is loaded via <script> in index.html
    // Models served via jsDelivr CDN (works with NetFree)
    const faceapi = (window as any).faceapi;
    const MODEL_URL = 'https://cdn.jsdelivr.net/npm/@vladmandic/face-api/model';
    await Promise.all([
      faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL),
      faceapi.nets.faceExpressionNet.loadFromUri(MODEL_URL),
    ]);
    this.faceApiLoaded = true;
  }

  private async startDetection() {
    const faceapi = (window as any).faceapi;
    const video = this.videoEl.nativeElement;
    video.srcObject = this.stream!;
    await video.play();

    const TIMEOUT_MS = 8000;
    const startTime = Date.now();

    // Detect once — as soon as a face is found, add emoji and close
    const detect = async () => {
      // Give up after 8 seconds
      if (Date.now() - startTime > TIMEOUT_MS) {
        this.input = this.input + '😐';
        this.stopCamera();
        return;
      }

      const result = await faceapi
        .detectSingleFace(video, new faceapi.TinyFaceDetectorOptions({ inputSize: 160, scoreThreshold: 0.4 }))
        .withFaceExpressions();

      if (result) {
        // Pick the dominant expression
        const expressions: Record<string, number> = result.expressions;
        const dominant = Object.entries(expressions)
          .sort((a, b) => b[1] - a[1])[0][0];

        const emoji = EMOTION_EMOJI[dominant] ?? '😐';
        this.input = this.input + emoji;  // append to whatever they typed
        this.stopCamera();
      } else {
        // No face yet — try again in 200ms
        if (this.cameraActive) setTimeout(detect, 200);
      }
    };

    detect();
  }

  private stopCamera() {
    this.stream?.getTracks().forEach(t => t.stop());
    this.stream = null;
    this.cameraActive = false;
  }

  ngOnDestroy() {
    this.stopCamera();
  }
}
