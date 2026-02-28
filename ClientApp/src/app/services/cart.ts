import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, catchError, throwError, map } from 'rxjs';
import { AuthService } from './auth';
import { Product } from '../models/product.model';

// ממשקים לעגלת קניות והזמנות
export interface CartItem {
  id: number;
  productId: number;
  product?: Product;
  quantity: number;
  price: number;
  totalPrice: number;
}

export interface Cart {
  id: number;
  userId: number;
  items: CartItem[];
  totalAmount: number;
  itemCount: number;
}

export interface Order {
  id: number;
  userId: number;
  orderDate: Date;
  totalAmount: number;
  status: OrderStatus;
  orderItems: OrderItem[];
}

export interface OrderItem {
  id: number;
  orderId: number;
  productId: number;
  product?: Product;
  quantity: number;
  price: number;
}

export enum OrderStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed', 
  Shipped = 'Shipped',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled'
}

// שירות לניהול עגלת קניות והזמנות
@Injectable({
  providedIn: 'root',
})
export class CartService {
  private apiUrl = 'http://localhost:5013/api';
  
  // מצב נוכחי של עגלת הקניות
  private cartSubject = new BehaviorSubject<Cart | null>(null);
  private cartItemCountSubject = new BehaviorSubject<number>(0);
  
  public cart$ = this.cartSubject.asObservable();
  public cartItemCount$ = this.cartItemCountSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {
    // האזנה לשינויים בסטטוס האימות
    this.authService.isLoggedIn$.subscribe(isLoggedIn => {
      if (isLoggedIn) {
        this.loadCart();
      } else {
        this.clearCart();
      }
    });
  }

  // קבלת עגלת קניות נוכחית
  getCart(): Observable<Cart> {
    const headers = this.getAuthHeaders();
    
    return this.http.get<Cart>(`${this.apiUrl}/Cart`, { headers })
      .pipe(
        map(cart => {
          this.cartSubject.next(cart);
          this.cartItemCountSubject.next(cart.itemCount);
          return cart;
        }),
        catchError(error => {
          console.error('Error fetching cart:', error);
          return throwError(() => error);
        })
      );
  }

  // הוספת מוצר לעגלה
  addToCart(productId: number, quantity: number = 1): Observable<CartItem> {
    const headers = this.getAuthHeaders();
    const body = { productId, quantity };
    
    return this.http.post<CartItem>(`${this.apiUrl}/Cart/add`, body, { headers })
      .pipe(
        map(cartItem => {
          // עדכון העגלה המקומית
          this.refreshCart();
          return cartItem;
        }),
        catchError(error => {
          console.error('Error adding to cart:', error);
          return throwError(() => error);
        })
      );
  }

  // עדכון כמות פריט בעגלה
  updateCartItem(cartItemId: number, quantity: number): Observable<CartItem> {
    const headers = this.getAuthHeaders();
    const body = { quantity };
    
    return this.http.put<CartItem>(`${this.apiUrl}/Cart/item/${cartItemId}`, body, { headers })
      .pipe(
        map(cartItem => {
          this.refreshCart();
          return cartItem;
        }),
        catchError(error => {
          console.error('Error updating cart item:', error);
          return throwError(() => error);
        })
      );
  }

  // הסרת פריט מהעגלה
  removeFromCart(cartItemId: number): Observable<void> {
    const headers = this.getAuthHeaders();
    
    return this.http.delete<void>(`${this.apiUrl}/Cart/item/${cartItemId}`, { headers })
      .pipe(
        map(() => {
          this.refreshCart();
        }),
        catchError(error => {
          console.error('Error removing from cart:', error);
          return throwError(() => error);
        })
      );
  }

  // ניקוי כל העגלה
  clearCart(): void {
    this.cartSubject.next(null);
    this.cartItemCountSubject.next(0);
  }

  // פניפת עגלה (מחיקה מהשרת)
  emptyCart(): Observable<void> {
    const headers = this.getAuthHeaders();
    
    return this.http.delete<void>(`${this.apiUrl}/Cart/clear`, { headers })
      .pipe(
        map(() => {
          this.clearCart();
        }),
        catchError(error => {
          console.error('Error clearing cart:', error);
          return throwError(() => error);
        })
      );
  }

  // ביצוע הזמנה מהעגלה הנוכחית
  checkout(): Observable<Order> {
    const headers = this.getAuthHeaders();
    
    return this.http.post<Order>(`${this.apiUrl}/Order/checkout`, {}, { headers })
      .pipe(
        map(order => {
          // ניקוי העגלה לאחר הזמנה מוצלחת
          this.clearCart();
          return order;
        }),
        catchError(error => {
          console.error('Error during checkout:', error);
          return throwError(() => error);
        })
      );
  }

  // קבלת היסטוריית הזמנות
  getOrders(): Observable<Order[]> {
    const headers = this.getAuthHeaders();
    
    return this.http.get<Order[]>(`${this.apiUrl}/Order`, { headers })
      .pipe(
        catchError(error => {
          console.error('Error fetching orders:', error);
          return throwError(() => error);
        })
      );
  }

  // קבלת הזמנה לפי ID
  getOrderById(orderId: number): Observable<Order> {
    const headers = this.getAuthHeaders();
    
    return this.http.get<Order>(`${this.apiUrl}/Order/${orderId}`, { headers })
      .pipe(
        catchError(error => {
          console.error('Error fetching order:', error);
          return throwError(() => error);
        })
      );
  }

  // שמירת עיצוב חדר כהזמנה מותאמת אישית
  saveRoomDesign(furnitureItems: any[], designName: string = 'Room Design'): Observable<Order> {
    const headers = this.getAuthHeaders();
    const body = {
      designName,
      furnitureItems: furnitureItems.map(item => ({
        productId: item.id,
        quantity: 1,
        position: { x: item.x, y: item.y },
        size: { width: item.width, height: item.height }
      }))
    };
    
    return this.http.post<Order>(`${this.apiUrl}/Order/room-design`, body, { headers })
      .pipe(
        catchError(error => {
          console.error('Error saving room design:', error);
          return throwError(() => error);
        })
      );
  }

  // קבלת עיצובי חדרים שמורים
  getSavedDesigns(): Observable<Order[]> {
    const headers = this.getAuthHeaders();
    
    return this.http.get<Order[]>(`${this.apiUrl}/Order/room-designs`, { headers })
      .pipe(
        catchError(error => {
          console.error('Error fetching saved designs:', error);
          return throwError(() => error);
        })
      );
  }

  // טעינת עגלה מהשרת
  private loadCart(): void {
    if (this.authService.isAuthenticated()) {
      this.getCart().subscribe({
        error: error => {
          console.error('Failed to load cart:', error);
          this.clearCart();
        }
      });
    }
  }

  // רענון העגלה מהשרת
  private refreshCart(): void {
    if (this.authService.isAuthenticated()) {
      this.getCart().subscribe({
        error: error => console.error('Failed to refresh cart:', error)
      });
    }
  }

  // יצירת כותרות בסיסיות
  private getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({ 'Content-Type': 'application/json' });
  }

  // פונקציות עזר לבדיקת סטטוס
  getCurrentCartValue(): Cart | null {
    return this.cartSubject.value;
  }

  getCurrentItemCount(): number {
    return this.cartItemCountSubject.value;
  }

  hasItems(): boolean {
    return this.getCurrentItemCount() > 0;
  }
}
