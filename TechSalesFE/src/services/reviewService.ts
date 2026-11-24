import { unsupportedFeature } from './unsupportedFeature';
import { Review, ReviewStatus } from '../models/ui_types/review';

export const reviewService = {
  getReviewsByProductId: async (
    productId: string,
    _pageNumber = 1,
    _pageSize = 100
  ): Promise<{ reviews: Review[]; averageRating: number; totalCount: number }> => { return unsupportedFeature("đánh giá"); },

  getAllReviews: async (_pageNumber = 1, _pageSize = 20): Promise<Review[]> => { return unsupportedFeature("đánh giá"); },

  submitReview: async (reviewData: {
    orderId: string;
    productId: string;
    ratingStars: number;
    reviewComment: string;
  }): Promise<void> => { return unsupportedFeature("đánh giá"); },

  moderateReview: async (id: string, status: ReviewStatus, _reason = 'Moderated by staff'): Promise<void> => { return unsupportedFeature("đánh giá"); },

  replyToReview: async (id: string, reply: string): Promise<void> => { return unsupportedFeature("đánh giá"); },
};
