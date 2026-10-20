import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { orderService, CheckoutPreviewParams } from '../services/orderService';

export const useGetOrders = () => {
  return useQuery({
    queryKey: ['orders'],
    queryFn: () => orderService.getOrders(),
  });
};

export const useGetAdminOrders = () => {
  return useQuery({
    queryKey: ['admin-orders'],
    queryFn: () => orderService.getAdminOrders(),
  });
};

export const useGetOrder = (id: string) => {
  return useQuery({
    queryKey: ['orders', id],
    queryFn: () => orderService.getOrderById(id),
    enabled: !!id,
  });
};

export const useGetAdminOrder = (id: string) => {
  return useQuery({
    queryKey: ['admin-orders', id],
    queryFn: () => orderService.getOrderStaffDetail(id),
    enabled: !!id,
  });
};

export const useCheckoutSummary = (params: CheckoutPreviewParams) => {
  return useQuery({
    queryKey: ['checkout-summary', params],
    queryFn: () => orderService.calculateCheckoutSummary(params),
    enabled: params.items.length > 0,
  });
};

export const useCreateOrder = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (orderData: any) => orderService.createOrder(orderData),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
};

export const useCancelOrder = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => orderService.cancelOrder(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['orders', id] });
    },
  });
};

export const useUpdateOrderStatus = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status, itemSerials }: { id: string; status: any; itemSerials?: Record<string, string[]> }) => 
      orderService.updateOrderStatus(id, status, itemSerials),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
      queryClient.invalidateQueries({ queryKey: ['orders', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['admin-orders', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['product-units'] });
    },
  });
};

export const useGetAvailableSerials = (productId: string) => {
  return useQuery({
    queryKey: ['available-serials', productId],
    queryFn: () => orderService.getAvailableSerials(productId),
    enabled: !!productId,
  });
};

export const useGetProductUnits = (productId: string) => {
  return useQuery({
    queryKey: ['product-units', productId],
    queryFn: () => orderService.getProductUnitsByProductId(productId),
    enabled: !!productId,
  });
};

export const useRepayOrder = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, paymentMethodId }: { id: string; paymentMethodId?: string }) =>
      orderService.repay(id, paymentMethodId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['orders', variables.id] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
};

export const useUpdatePaymentMethod = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ orderId, paymentMethodId }: { orderId: string; paymentMethodId: string }) =>
      orderService.updatePaymentMethod(orderId, paymentMethodId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
      queryClient.invalidateQueries({ queryKey: ['orders', variables.orderId] });
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
      queryClient.invalidateQueries({ queryKey: ['admin-orders', variables.orderId] });
    },
  });
};
