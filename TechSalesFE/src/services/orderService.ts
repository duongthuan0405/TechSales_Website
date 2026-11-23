import { paymentService } from './paymentService';
import { productService } from './productService';
import { voucherService } from './voucherService';
import { ApiError } from '../api/apiClient';
import { Order, OrderStatus, OrderItem, ProductUnit } from '../models/ui_types/order';
import { orders, products, mockAddresses, paymentMethods, vouchers, mockCart, mockProductUnits } from '../data/mockData';
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

const delay = (ms = 50) => new Promise(resolve => setTimeout(resolve, ms));

export const orderService = {
  // ── Customer APIs ────────────────────────────────────────
  getOrders: async (_pageNumber = 1, _pageSize = 10): Promise<Order[]> => {
    await delay();
    return [...orders];
  },

  getOrderById: async (id: string): Promise<Order> => {
    await delay();
    const order = orders.find(o => o.id === id);
    if (!order) {
      throw new Error(`Order ${id} not found`);
    }
    return { ...order };
  },

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

  cancelOrder: async (id: string): Promise<void> => {
    await delay();
    const order = orders.find(o => o.id === id);
    if (order) {
      if (order.status === OrderStatus.DELIVERED || order.status === OrderStatus.SHIPPING) {
        throw new Error('Không thể hủy đơn hàng khi đơn đã được giao hoặc đang giao. Vui lòng sử dụng tính năng hoàn trả hàng.');
      }
      order.status = OrderStatus.CANCELLED;
      order.updatedAt = new Date().toISOString();
      if (order.payments) {
        order.payments.forEach(p => (p.status = 'CANCELLED'));
      }
    }
  },

  updateOrderStatus: async (
    id: string, 
    status: OrderStatus, 
    itemSerials?: Record<string, string[]>
  ): Promise<void> => {
    await delay();
    const order = orders.find(o => o.id === id);
    if (!order) {
      throw new Error(`Order ${id} not found`);
    }

    if (status === OrderStatus.CANCELLED && (order.status === OrderStatus.DELIVERED || order.status === OrderStatus.SHIPPING)) {
      throw new Error('Không thể hủy đơn hàng khi đơn đã được giao hoặc đang giao. Vui lòng sử dụng tính năng hoàn trả hàng.');
    }

    // Quy trình duyệt đơn hàng (PENDING -> APPROVED): Bắt buộc gán số serie cho từng thiết bị
    if (status === OrderStatus.APPROVED && order.items && order.items.length > 0) {
      if (itemSerials) {
        for (const item of order.items) {
          const serials = itemSerials[item.productId] || [];
          if (serials.length !== item.quantity) {
            throw new Error(`Sản phẩm "${item.productName || item.productId}" yêu cầu nhập đủ ${item.quantity} số serie (hiện tại: ${serials.length}).`);
          }
          if (serials.some(s => !s || !s.trim())) {
            throw new Error(`Sản phẩm "${item.productName || item.productId}" có số serie đang để trống. Vui lòng điền đủ cho từng thiết bị.`);
          }

          // Kiểm tra trùng lặp trong cùng item
          const uniqueSerials = new Set(serials.map(s => s.trim().toUpperCase()));
          if (uniqueSerials.size !== serials.length) {
            throw new Error(`Sản phẩm "${item.productName || item.productId}" có số serie bị trùng lặp.`);
          }

          item.serialNumbers = serials.map(s => s.trim().toUpperCase());

          // Đánh dấu hoặc thêm ProductUnit vào kho
          item.serialNumbers.forEach(sn => {
            const existingUnit = mockProductUnits.find(u => u.serialNumber.toUpperCase() === sn);
            if (existingUnit) {
              existingUnit.status = 'ASSIGNED';
              existingUnit.orderId = order.id;
              existingUnit.assignedAt = new Date().toISOString();
            } else {
              mockProductUnits.push({
                serialNumber: sn,
                productId: item.productId,
                productName: item.productName,
                orderId: order.id,
                status: 'ASSIGNED',
                warrantyMonths: 12,
                assignedAt: new Date().toISOString()
              });
            }
          });
        }
      } else {
        // Nếu không truyền itemSerials, kiểm tra xem đơn hàng đã được gán đủ serial từ trước chưa
        const missing = order.items.find(
          item => !item.serialNumbers || item.serialNumbers.length !== item.quantity || item.serialNumbers.some(s => !s || !s.trim())
        );
        if (missing) {
          throw new Error(
            `Quy trình duyệt đơn yêu cầu nhập đủ số serie cho từng thiết bị. Sản phẩm "${missing.productName || missing.productId}" còn thiếu số serie.`
          );
        }
      }
    }

    order.status = status;
    order.updatedAt = new Date().toISOString();

    if (order.payments && (status === OrderStatus.DELIVERED || status === OrderStatus.APPROVED)) {
      order.payments.forEach(p => (p.status = 'SUCCESS'));
    } else if (order.payments && status === OrderStatus.CANCELLED) {
      order.payments.forEach(p => (p.status = 'CANCELLED'));
    }
  },

  getAvailableSerials: async (productId: string): Promise<string[]> => {
    await delay(30);
    return mockProductUnits
      .filter(u => u.productId === productId && u.status === 'AVAILABLE')
      .map(u => u.serialNumber);
  },

  getProductUnitsByProductId: async (productId: string): Promise<ProductUnit[]> => {
    await delay(30);
    return mockProductUnits.filter(u => u.productId === productId);
  },

  // ── Staff & Admin APIs ────────────────────────────────────
  getPendingOrders: async (_pageNumber = 1, _pageSize = 20): Promise<Order[]> => {
    await delay();
    return orders.filter(o => o.status === OrderStatus.PENDING);
  },

  getOrderStaffDetail: async (id: string): Promise<Order> => {
    await delay();
    const order = orders.find(o => o.id === id);
    if (!order) {
      throw new Error(`Order ${id} not found`);
    }
    return { ...order };
  },

  searchOrders: async (params: {
    orderCode?: string;
    customerName?: string;
    phoneNumber?: string;
    fromDate?: string;
    toDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<Order[]> => {
    await delay();
    let result = [...orders];

    if (params.orderCode) {
      const code = params.orderCode.toLowerCase();
      result = result.filter(o => o.id.toLowerCase().includes(code));
    }

    if (params.customerName) {
      const name = params.customerName.toLowerCase();
      result = result.filter(o => o.customerName?.toLowerCase().includes(name));
    }

    return result;
  },

  initiateRefund: async (id: string): Promise<void> => {
    await delay();
    const order = orders.find(o => o.id === id);
    if (order && order.payments) {
      order.payments.forEach(p => (p.status = 'CANCELLED'));
    }
  },

  getAdminOrders: async (_pageNumber = 1, _pageSize = 20): Promise<Order[]> => {
    await delay();
    return [...orders];
  },

  repay: async (id: string, _paymentMethodId?: string): Promise<string | null> => {
    const pending = (await paymentService.getOrderPayments(id)).find(payment => payment.status === 'PENDING');
    if (!pending) throw new ApiError('Không có giao dịch đang chờ thanh toán.', 404);
    return paymentService.checkout(pending.id);
  },

  updatePaymentMethod: async (orderId: string, paymentMethodId: string): Promise<void> => {
    await paymentService.replaceOrderPayment(orderId, paymentMethodId);
  },
};
