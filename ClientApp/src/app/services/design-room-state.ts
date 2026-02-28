import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { CanvasItem } from '../models/canvas.model';

@Injectable({ providedIn: 'root' })
export class DesignRoomStateService {

  // Storage key per user — set via setUser()
  private storageKey = 'designRoom_canvas_guest';
  // Guests use sessionStorage (cleared on tab close); logged-in users use localStorage
  private useSession = true;

  // BehaviorSubject starts empty until setUser() loads the correct user's canvas
  private itemsSubject = new BehaviorSubject<CanvasItem[]>([]);
  public items$: Observable<CanvasItem[]> = this.itemsSubject.asObservable();

  // ---------- user identity ----------

  /**
   * Call once the user identity is known (in CanvasComponent.ngOnInit).
   * - userId !== null  → localStorage key  `designRoom_canvas_${userId}`
   * - userId === null  → sessionStorage key `designRoom_canvas_guest`
   *   (session storage is cleared automatically when the browser tab is closed)
   */
  setUser(userId: number | null): void {
    if (userId !== null) {
      this.storageKey = `designRoom_canvas_${userId}`;
      this.useSession = false;
    } else {
      this.storageKey = 'designRoom_canvas_guest';
      this.useSession = true;
    }
    // Load whatever this user had saved
    this.itemsSubject.next(this.loadFromStorage());
  }

  // ---------- read helpers ----------

  getItems(): CanvasItem[] {
    return this.itemsSubject.value;
  }

  // ---------- write helpers ----------

  addItem(item: CanvasItem): void {
    this.publish([...this.itemsSubject.value, item]);
  }

  moveItem(id: string, x: number, y: number): void {
    this.publish(this.itemsSubject.value.map(item =>
      item.id === id ? { ...item, x, y } : item
    ));
  }

  resizeItem(id: string, width: number, height: number): void {
    this.publish(this.itemsSubject.value.map(item =>
      item.id === id
        ? { ...item, width: Math.max(40, width), height: Math.max(40, height) }
        : item
    ));
  }

  removeItem(id: string): void {
    this.publish(this.itemsSubject.value.filter(item => item.id !== id));
  }

  clearCanvas(): void {
    this.publish([]);
  }

  // ---------- private helpers ----------

  private publish(items: CanvasItem[]): void {
    this.itemsSubject.next(items);
    this.saveToStorage(items);
  }

  private storage(): Storage {
    return this.useSession ? sessionStorage : localStorage;
  }

  private saveToStorage(items: CanvasItem[]): void {
    try {
      this.storage().setItem(this.storageKey, JSON.stringify(items));
    } catch {
      console.warn('Storage not available.');
    }
  }

  private loadFromStorage(): CanvasItem[] {
    try {
      const raw = this.storage().getItem(this.storageKey);
      return raw ? (JSON.parse(raw) as CanvasItem[]) : [];
    } catch {
      return [];
    }
  }
}
