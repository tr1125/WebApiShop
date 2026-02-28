import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OrderDTO, OrderItemDTO } from '../models/order.model';

const API = 'http://localhost:5013/api';

@Injectable({ providedIn: 'root' })
export class OrderService {

  constructor(private http: HttpClient) {}

  getOrdersByUser(userId: number): Observable<OrderDTO[]> {
    return this.http.get<OrderDTO[]>(`${API}/Orders/user/${userId}`);
  }

  getAllOrders(): Observable<OrderDTO[]> {
    return this.http.get<OrderDTO[]>(`${API}/Orders`);
  }

  addOrder(order: Partial<OrderDTO>): Observable<OrderDTO> {
    const payload = {
      ...order,
      orderDate: order.orderDate instanceof Date 
        ? order.orderDate.toISOString().split('T')[0]
        : order.orderDate
    };
    return this.http.post<OrderDTO>(`${API}/Orders`, payload);
  }

  updateOrderStatus(orderId: number, status: string): Observable<boolean> {
    return this.http.put<boolean>(`${API}/Orders/${orderId}/status`, { Status: status });
  }
}
