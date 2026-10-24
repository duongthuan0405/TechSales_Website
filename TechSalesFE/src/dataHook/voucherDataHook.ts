import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { voucherService } from '../services/voucherService';

export const useGetVouchers = (page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: ['vouchers', page, pageSize],
    queryFn: () => voucherService.getVouchers(page, pageSize),
  });
};

export const useCreateVoucher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: voucherService.createVoucher,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vouchers'] });
    },
  });
};

export const useDeleteVoucher = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: voucherService.deleteVoucher,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vouchers'] });
    },
  });
};

export const useGetAvailableVouchers = () => {
  return useQuery({
    queryKey: ['vouchers', 'available'],
    queryFn: () => voucherService.getAvailableVouchers(),
  });
};

export const useValidateVoucher = () => {
  return useMutation({
    mutationFn: ({ code, orderAmount }: { code: string; orderAmount: number }) => 
      voucherService.validateVoucher(code, orderAmount),
  });
};
