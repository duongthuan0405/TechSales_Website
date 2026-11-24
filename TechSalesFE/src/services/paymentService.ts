import { PaymentMethod } from '../models/ui_types/paymentMethod';
import api, { ApiError } from '../api/apiClient';

const requireRealOrder = (id: string) => {
  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id))
    throw new ApiError('Đơn hàng mẫu chưa thể thanh toán. Cần đơn hàng thật từ hệ thống.', 400);
};
export const paymentService = {
  getOrderPayments: async (orderId: string) => {
    requireRealOrder(orderId);
    return (await api.get<{ items: { id: string; status: string; transactionMethodId: string }[] }>(
      `/api/payments/by-ref/${encodeURIComponent(orderId)}?type=ORDER&pageSize=100`)).items;
  },
  replaceOrderPayment: async (orderId: string, methodId: string): Promise<string> => {
    requireRealOrder(orderId);
    const result = await api.post<{ paymentId: string }>('/api/payments/replace', {
      type: 'ORDER', refId: orderId, transactionMethodId: methodId });
    return result.paymentId;
  },
  checkout: async (paymentId: string): Promise<string> => {
    const result = await api.post<{ paymentUrl: string }>(`/api/payments/${encodeURIComponent(paymentId)}/checkout`);
    return result.paymentUrl;
  },
  getPaymentMethods: async (): Promise<PaymentMethod[]> => {
    const methods = await api.get<{ id: string; name: string; type: string; code: string | null }[]>('/api/admin/transaction-methods');
    return methods.map(method => ({ ...method, description: method.code ?? method.type, isActive: true }));
  },
};
