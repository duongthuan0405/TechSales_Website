import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { reviewService } from '../services/reviewService';
import { Review, ReviewStatus } from '../models/ui_types/review';

export const useGetProductReviews = (productId: string) => {
  return useQuery({
    queryKey: ['reviews', productId],
    queryFn: () => reviewService.getReviewsByProductId(productId),
    enabled: !!productId,
  });
};

export const useGetAllReviews = () => {
  return useQuery({
    queryKey: ['reviews', 'all'],
    queryFn: () => reviewService.getAllReviews(),
  });
};

export const useSubmitReview = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (reviewData: { orderId: string; productId: string; ratingStars: number; reviewComment: string }) => 
      reviewService.submitReview(reviewData),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['reviews', variables.productId] });
      queryClient.invalidateQueries({ queryKey: ['reviews', 'all'] });
      queryClient.invalidateQueries({ queryKey: ['product', variables.productId] });
    },
  });
};

export const useModerateReview = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, status }: { id: string, status: ReviewStatus }) => 
      reviewService.moderateReview(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reviews'] });
    },
  });
};

export const useReplyToReview = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, reply }: { id: string, reply: string }) => 
      reviewService.replyToReview(id, reply),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reviews'] });
    },
  });
};
