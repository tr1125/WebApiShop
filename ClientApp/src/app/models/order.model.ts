export interface OrderItemDTO {
  productId: number;
  quantity: number | null;
  productName: string | null;
  price: number | null;
}

export interface OrderDTO {
  orderId: number;
  orderDate: string | Date;
  orderSum: number;
  orderItems: OrderItemDTO[];
  status: string;
  userId?: number;
}

export const ORDER_STATUSES: { value: string; label: string }[] = [
  { value: 'Pending',   label: 'ממתין'   },
  { value: 'Confirmed', label: 'אושר'    },
  { value: 'Shipped',   label: 'נשלח'    },
  { value: 'Delivered', label: 'נמסר'    },
  { value: 'Cancelled', label: 'בוטל'    },
];
