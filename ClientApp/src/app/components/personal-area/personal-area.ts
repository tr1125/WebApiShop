import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { AuthService } from '../../services/auth';
import { DesignRoomStateService } from '../../services/design-room-state';
import { CanvasItem } from '../../models/canvas.model';
import { OrderService } from '../../services/order';
import { OrderDTO, ORDER_STATUSES } from '../../models/order.model';
import { ProductService } from '../../services/product';

@Component({
  selector: 'app-personal-area',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './personal-area.html',
  styleUrls: ['./personal-area.scss'],
})
export class PersonalAreaComponent implements OnInit, OnDestroy {

  // ── Cart (right panel) ──────────────────────────────────────────
  cartItems: CanvasItem[] = [];
  isSubmitting = false;
  submitError: string | null = null;
  submitSuccess = false;

  // ── Past orders (left panel) ──────────────────────────────────
  pastOrders: OrderDTO[] = [];
  isLoadingOrders = false;
  ordersError: string | null = null;

  // Status editing: orderId → selected status
  pendingStatus: { [orderId: number]: string } = {};

  readonly statuses = ORDER_STATUSES;

  private subs = new Subscription();

  constructor(
    private authService: AuthService,
    private stateService: DesignRoomStateService,
    private orderService: OrderService,
    private productService: ProductService,
    public  router: Router,
  ) {}

  // ── Lifecycle ──────────────────────────────────────────────────

  ngOnInit(): void {
    // Redirect guests to auth
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth']);
      return;
    }

    // Sync user storage key (in case coming directly to this route)
    const user = this.authService.getCurrentUser();
    this.stateService.setUser(user?.userId ?? null);

    // Subscribe to canvas items → filter furniture only
    this.subs.add(
      this.stateService.items$.subscribe(items => {
        this.cartItems = items.filter(i => i.type !== 'wall' && i.type !== 'floor');
      })
    );

    // Load past orders
    this.loadOrders();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // ── Cart helpers ──────────────────────────────────────────────

  get currentUser() { return this.authService.getCurrentUser(); }
  get isAdmin() { return this.authService.isAdmin(); }

  get cartTotal(): number {
    return this.cartItems.reduce((sum, item) => sum + (item.price ?? 0), 0);
  }

  get canSubmit(): boolean {
    return this.cartItems.length > 0 && !this.isSubmitting;
  }

  // ── Close order (checkout) ────────────────────────────────────

  closeOrder(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth']);
      return;
    }
    if (!this.canSubmit) return;
    const user = this.currentUser;
    if (!user) return;

    this.isSubmitting = true;
    this.submitError  = null;
    this.submitSuccess = false;

    // Group items by productId and sum quantities
    const grouped = new Map<number, number>();
    for (const item of this.cartItems) {
      if (item.productId) {
        grouped.set(item.productId, (grouped.get(item.productId) ?? 0) + 1);
      }
    }

    const today = new Date();
    const orderPayload: Partial<OrderDTO> = {
      orderId: 0,
      orderDate: new Date(today.getFullYear(), today.getMonth(), today.getDate()),
      orderSum:  this.cartTotal,
      status:    'Pending',
      userId:    user.userId,
      orderItems: Array.from(grouped.entries()).map(([productId, quantity]) => ({
        productId,
        quantity,
        productName: null,
        price: null,
      })),
    };

    this.orderService.addOrder(orderPayload).subscribe({
      next: () => {
        this.isSubmitting   = false;
        this.submitSuccess  = true;
        this.stateService.clearCanvas();
        this.loadOrders();
      },
      error: err => {
        this.isSubmitting = false;
        this.submitError  = 'שגיאה בשליחת ההזמנה. נסה שוב.';
        console.error(err);
      },
    });
  }

  // ── Past orders helpers ───────────────────────────────────────

  loadOrders(): void {
    const user = this.currentUser;
    if (!user) return;

    this.isLoadingOrders = true;
    this.ordersError     = null;

    const obs = this.isAdmin
      ? this.orderService.getAllOrders()
      : this.orderService.getOrdersByUser(user.userId);

    obs.subscribe({
      next: orders => {
        this.pastOrders      = orders;
        this.isLoadingOrders = false;
        // Pre-fill pending status selects with current status
        for (const o of orders) {
          this.pendingStatus[o.orderId] = o.status;
        }
      },
      error: err => {
        this.ordersError     = 'שגיאה בטעינת ההזמנות.';
        this.isLoadingOrders = false;
        console.error(err);
      },
    });
  }

  updateStatus(order: OrderDTO): void {
    const newStatus = this.pendingStatus[order.orderId];
    if (!newStatus || newStatus === order.status) return;

    this.orderService.updateOrderStatus(order.orderId, newStatus).subscribe({
      next: () => {
        order.status = newStatus;
      },
      error: err => console.error('Status update failed', err),
    });
  }

  statusLabel(status: string): string {
    return this.statuses.find(s => s.value === status)?.label ?? status;
  }

  statusColor(status: string): string {
    const map: Record<string, string> = {
      Pending:   'bg-yellow-100 text-yellow-800 border-yellow-200',
      Confirmed: 'bg-blue-100 text-blue-800 border-blue-200',
      Shipped:   'bg-blue-100 text-blue-800 border-blue-200',
      Delivered: 'bg-green-100 text-green-800 border-green-200',
      Cancelled: 'bg-red-100 text-red-800 border-red-200',
    };
    return map[status] ?? 'bg-gray-100 text-gray-600 border-gray-200';
  }

  // ── Navigation ────────────────────────────────────────────────

  backToDesign(): void {
    this.router.navigate(['/design']);
  }

  getImageUrl(relativePath: string): string {
    return this.productService.getImageUrl(relativePath);
  }
}
