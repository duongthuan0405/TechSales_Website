import { unsupportedFeature } from './unsupportedFeature';
import { paymentService } from './paymentService';
import { productService } from './productService';
import { voucherService } from './voucherService';
import { ApiError } from '../api/apiClient';
import { Order, OrderStatus, ProductUnit } from '../models/ui_types/order';

import { VoucherType } from '../models/ui_types/voucher';

export interface CheckoutPreviewParams {
  items: { productId: string; quantity: number }[];
  couponCode?: string;
}

export interface CheckoutSummary {
  subtotal: number;
  shippingFee: number;
  total: number;
  discount: number;
}

export const orderService = {
  // ── Customer APIs ────────────────────────────────────────
  getOrders: async (_pageNumber = 1, _pageSize = 10): Promise<Order[]> => { return unsupportedFeature("đơn hàng"); },

  getOrderById: async (id: string): Promise<Order> => { return unsupportedFeature("đơn hàng"); },

  calculateCheckoutSummary: async (params: CheckoutPreviewParams): Promise<CheckoutSummary> => {
    const current = params.items.length ? await productService.getProducts() : [];
    let subtotal = 0;
    for (const item of params.items) {
      const product = current.find(product => product.id === item.productId);
      if (!product || product.priceIsUnset) throw new ApiError('Có sản phẩm không khả dụng trong đơn hàng.', 400);
      subtotal += product.price * item.quantity;
    }
    let discount = 0;
    if (params.couponCode) {
      const voucher = await voucherService.validateVoucher(params.couponCode, subtotal);
      discount = voucher.type === VoucherType.PERCENTAGE ? Math.floor(subtotal * voucher.value / 100) : voucher.value;
    }
    discount = Math.min(discount, subtotal);
    // Shipping is quoted separately by the Shipping API once an address is selected.
    return { subtotal, shippingFee: 0, discount, total: subtotal - discount };
  },

  createOrder: async (orderData: {
    productsWithQuantity: Record<string, number>;
    shippingAddressId: string;
    paymentMethodId: string;
    voucherCode?: string;
  }): Promise<Order> => {
    throw new ApiError('Chưa hỗ trợ đặt hàng trực tuyến. Giỏ hàng của bạn vẫn được giữ nguyên.', 501);
  },

  cancelOrder: async (id: string): Promise<void> => { return unsupportedFeature("đơn hàng"); },

  updateOrderStatus: async (
    id: string, 
    status: OrderStatus, 
    itemSerials?: Record<string, string[]>
  ): Promise<void> => { return unsupportedFeature("đơn hàng"); },

  getAvailableSerials: async (productId: string): Promise<string[]> => { return unsupportedFeature("đơn hàng"); },

  getProductUnitsByProductId: async (productId: string): Promise<ProductUnit[]> => { return unsupportedFeature("đơn hàng"); },

  // ── Staff & Admin APIs ────────────────────────────────────
  getPendingOrders: async (_pageNumber = 1, _pageSize = 20): Promise<Order[]> => { return unsupportedFeature("đơn hàng"); },

  getOrderStaffDetail: async (id: string): Promise<Order> => { return unsupportedFeature("đơn hàng"); },

  searchOrders: async (params: {
    orderCode?: string;
    customerName?: string;
    phoneNumber?: string;
    fromDate?: string;
    toDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<Order[]> => { return unsupportedFeature("đơn hàng"); },

  initiateRefund: async (id: string): Promise<void> => { return unsupportedFeature("đơn hàng"); },

  getAdminOrders: async (_pageNumber = 1, _pageSize = 20): Promise<Order[]> => { return unsupportedFeature("đơn hàng"); },

  repay: async (id: string, _paymentMethodId?: string): Promise<string | null> => {
    const pending = (await paymentService.getOrderPayments(id)).find(payment => payment.status === 'PENDING');
    if (!pending) throw new ApiError('Không có giao dịch đang chờ thanh toán.', 404);
    return paymentService.checkout(pending.id);
  },

  updatePaymentMethod: async (orderId: string, paymentMethodId: string): Promise<void> => {
    await paymentService.replaceOrderPayment(orderId, paymentMethodId);
  },
};
