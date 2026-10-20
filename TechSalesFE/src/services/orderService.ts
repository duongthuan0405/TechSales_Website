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
    await delay(20);
    let subtotal = 0;
    for (const item of params.items) {
      const product = products.find(p => p.id === item.productId);
      if (product) {
        subtotal += product.price * item.quantity;
      }
    }

    let discount = 0;
    if (params.couponCode) {
      const code = params.couponCode.trim().toUpperCase();
      const voucher = vouchers.find(v => v.code.toUpperCase() === code && v.isActive);
      if (voucher && subtotal >= voucher.minOrderAmount) {
        if (voucher.type === VoucherType.PERCENTAGE) {
          discount = Math.round((subtotal * voucher.value) / 100);
        } else {
          discount = voucher.value;
        }
        discount = Math.min(discount, subtotal);
      }
    }

    const shippingFee = subtotal > 1500 || subtotal === 0 ? 0 : 25;
    const total = Math.max(0, subtotal + shippingFee - discount);

    return {
      subtotal,
      shippingFee,
      total,
      discount,
    };
  },

  createOrder: async (orderData: {
    productsWithQuantity: Record<string, number>;
    shippingAddressId: string;
    paymentMethodId: string;
    voucherCode?: string;
  }): Promise<Order> => {
    await delay();

    let subtotal = 0;
    const orderItems: OrderItem[] = [];
    const orderId = `ORD-${Math.floor(100 + Math.random() * 900)}`;

    for (const [productId, quantity] of Object.entries(orderData.productsWithQuantity)) {
      const product = products.find(p => p.id === productId);
      if (product) {
        const itemPrice = product.price;
        subtotal += itemPrice * quantity;
        orderItems.push({
          orderId,
          productId,
          productName: product.name,
          imageUrl: product.imageUrl,
          price: itemPrice,
          quantity,
        });

        // Deduct inventory in RAM
        product.stock = Math.max(0, (product.stock || 0) - quantity);
      }
    }

    let discount = 0;
    if (orderData.voucherCode) {
      const voucher = vouchers.find(
        v => v.code.toUpperCase() === orderData.voucherCode?.trim().toUpperCase() && v.isActive
      );
      if (voucher && subtotal >= voucher.minOrderAmount) {
        if (voucher.type === VoucherType.PERCENTAGE) {
          discount = Math.round((subtotal * voucher.value) / 100);
        } else {
          discount = voucher.value;
        }
        discount = Math.min(discount, subtotal);
        voucher.usedCount = (voucher.usedCount || 0) + 1;
      }
    }

    const shippingFee = subtotal > 1500 ? 0 : 25;
    const totalAmount = Math.max(0, subtotal + shippingFee - discount);

    // Look up address snapshot
    const addr = mockAddresses.find(a => a.id === orderData.shippingAddressId) || mockAddresses[0];
    const shippingAddressSnapshot = addr
      ? `${addr.detail}, ${addr.ward}, ${addr.province}`
      : '123 Tech Avenue, Ben Nghe Ward, Ho Chi Minh City';

    // Look up payment method
    const pm = paymentMethods.find(p => p.id === orderData.paymentMethodId) || paymentMethods[0];
    const paymentMethodName = pm ? pm.name : 'Credit Card';

    // Current user info
    let customerName = 'Demo Customer';
    let userId = 'u4';
    try {
      const rawUser = localStorage.getItem('tech_sales_user');
      if (rawUser) {
        const parsed = JSON.parse(rawUser);
        if (parsed.name) customerName = parsed.name;
        if (parsed.id) userId = parsed.id;
      }
    } catch {}

    const newOrder: Order = {
      id: orderId,
      userId,
      customerName,
      status: OrderStatus.PENDING,
      totalProductAmount: subtotal,
      shippingFee,
      discountAmount: discount,
      totalAmount,
      shippingAddressSnapshot,
      paymentMethodId: orderData.paymentMethodId,
      paymentMethodName,
      createdAt: new Date().toISOString(),
      items: orderItems,
      payments: [
        {
          id: `pay-${Date.now()}`,
          paymentMethodName,
          status: 'PENDING',
          amount: totalAmount,
        },
      ],
    };

    orders.unshift(newOrder);

    // Clear cart in RAM after successful order
    mockCart.length = 0;

    return newOrder;
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
    await delay();
    const order = orders.find(o => o.id === id);
    if (order) {
      if (order.payments) {
        order.payments.forEach(p => (p.status = 'SUCCESS'));
      }
      order.isPaymentFailed = false;
    }
    return null;
  },

  updatePaymentMethod: async (orderId: string, paymentMethodId: string): Promise<Order> => {
    await delay();
    const order = orders.find(o => o.id === orderId);
    if (!order) {
      throw new Error(`Không tìm thấy đơn hàng #${orderId}`);
    }
    if (order.status !== OrderStatus.PENDING) {
      throw new Error('Chỉ có thể thay đổi phương thức thanh toán khi đơn hàng chưa được duyệt (trạng thái Chờ xử lý).');
    }
    const pm = paymentMethods.find(p => p.id === paymentMethodId);
    if (!pm) {
      throw new Error('Phương thức thanh toán không hợp lệ');
    }
    order.paymentMethodId = pm.id;
    order.paymentMethodName = pm.name;

    if (order.payments && order.payments.length > 0) {
      order.payments = order.payments.map(p => {
        if (p.status !== 'SUCCESS') {
          return {
            ...p,
            paymentMethodName: pm.name,
            status: 'PENDING'
          };
        }
        return p;
      });
    } else {
      order.payments = [{
        id: `pay-${Date.now()}`,
        paymentMethodName: pm.name,
        status: 'PENDING',
        amount: order.totalAmount
      }];
    }
    order.updatedAt = new Date().toISOString();
    return { ...order };
  },
};
