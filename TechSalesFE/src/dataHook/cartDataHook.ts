import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { cartService } from '../services/cartService';

export const useGetCart = () => {
  return useQuery({
    queryKey: ['cart'],
    queryFn: () => cartService.getCart(),
  });
};

export const useAddToCart = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: string | { productId: string; quantity: number }) => typeof input === 'string'
      ? cartService.addToCart(input) : cartService.addToCart(input.productId, input.quantity),
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['checkout-summary'] });
    }
  });
};

export const useUpdateCartQuantity = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ productId, quantity }: { productId: string, quantity: number }) => 
      cartService.updateQuantity(productId, quantity),
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    }
  });
};

export const useRemoveFromCart = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (productId: string) => cartService.removeItem(productId),
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    }
  });
};

export const useClearCart = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => cartService.clearCart(),
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
    }
  });
};
