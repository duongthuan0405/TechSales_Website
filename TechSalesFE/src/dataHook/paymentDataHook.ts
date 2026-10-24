import { useQuery } from '@tanstack/react-query';
import { paymentService } from '../services/paymentService';

export const useGetPaymentMethods = () => {
  return useQuery({
    queryKey: ['paymentMethods'],
    queryFn: () => paymentService.getPaymentMethods(),
  });
};
