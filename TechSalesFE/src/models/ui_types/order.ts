export enum OrderStatus {
  PENDING = 'PENDING',
  APPROVED = 'APPROVED',
  SHIPPING = 'SHIPPING',
  DELIVERED = 'DELIVERED',
  CANCELLED = 'CANCELLED'
}

export interface ProductUnit {
  serialNumber: string;
  productId: string;
  productName?: string;
  orderId?: string;
  status: 'AVAILABLE' | 'ASSIGNED' | 'SOLD' | 'RETURNED';
  warrantyMonths?: number;
  assignedAt?: string;
}

export interface OrderItem {
  orderId: string;
  productId: string;
  productName?: string; // UI convenience
  imageUrl?: string; // UI convenience
  price: number; // Snapshot price
  quantity: number;
  serialNumbers?: string[]; // Danh sách số serial / ProductUnit được gán cho từng thiết bị
}

export interface Order {
  id: string;
  userId: string;
  status: OrderStatus;
  totalProductAmount: number;
  shippingFee: number;
  discountAmount: number;
  totalAmount: number;
  shippingAddressSnapshot: string;
  createdAt: string;
  updatedAt?: string;
  
  items?: OrderItem[];
  customerName?: string; // UI convenience
  paymentMethodId?: string; // Relation in DB
  paymentMethodName?: string; // UI convenience
  isPaymentFailed?: boolean | null; // UI convenience
  checkoutUrl?: string; // Redirect link for online payment
  payments?: PaymentInfo[];
}

export interface PaymentInfo {
  id: string;
  paymentMethodName: string;
  status: 'PENDING' | 'SUCCESS' | 'FAILED' | 'CANCELLED';
  amount: number;
  transactionRef?: string;
}
