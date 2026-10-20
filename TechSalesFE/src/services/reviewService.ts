import { Review, ReviewStatus } from '../models/ui_types/review';
import { mockReviews, products } from '../data/mockData';

const delay = (ms = 40) => new Promise(resolve => setTimeout(resolve, ms));

export const reviewService = {
  getReviewsByProductId: async (
    productId: string,
    _pageNumber = 1,
    _pageSize = 100
  ): Promise<{ reviews: Review[]; averageRating: number; totalCount: number }> => {
    await delay();
    const productReviews = mockReviews.filter(
      r => r.productId === productId && r.status === ReviewStatus.VISIBLE
    );

    const totalCount = productReviews.length;
    const sumRating = productReviews.reduce((sum, r) => sum + r.rating, 0);
    const averageRating = totalCount > 0 ? parseFloat((sumRating / totalCount).toFixed(1)) : 5.0;

    return {
      reviews: productReviews,
      averageRating,
      totalCount,
    };
  },

  getAllReviews: async (_pageNumber = 1, _pageSize = 20): Promise<Review[]> => {
    await delay();
    return [...mockReviews];
  },

  submitReview: async (reviewData: {
    orderId: string;
    productId: string;
    ratingStars: number;
    reviewComment: string;
  }): Promise<void> => {
    await delay();

    let userName = 'Demo Customer';
    let userId = 'u4';
    let userAvatar: string | undefined;

    try {
      const rawUser = localStorage.getItem('tech_sales_user');
      if (rawUser) {
        const parsed = JSON.parse(rawUser);
        if (parsed.name) userName = parsed.name;
        if (parsed.id) userId = parsed.id;
        userAvatar = parsed.avatarUrl;
      }
    } catch {}

    const product = products.find(p => p.id === reviewData.productId);

    const newRev: Review = {
      id: `rev-${Date.now()}`,
      userId,
      userName,
      userAvatar,
      productId: reviewData.productId,
      productName: product ? product.name : undefined,
      rating: reviewData.ratingStars,
      comment: reviewData.reviewComment,
      status: ReviewStatus.VISIBLE,
      createdAt: new Date().toISOString(),
      responses: [],
    };

    mockReviews.unshift(newRev);

    // Update product rating and reviewsCount in RAM
    if (product) {
      const productReviews = mockReviews.filter(
        r => r.productId === reviewData.productId && r.status === ReviewStatus.VISIBLE
      );
      product.reviewsCount = productReviews.length;
      product.rating = parseFloat(
        (productReviews.reduce((sum, r) => sum + r.rating, 0) / productReviews.length).toFixed(1)
      );
    }
  },

  moderateReview: async (id: string, status: ReviewStatus, _reason = 'Moderated by staff'): Promise<void> => {
    await delay();
    const review = mockReviews.find(r => r.id === id);
    if (review) {
      review.status = status;
    }
  },

  replyToReview: async (id: string, reply: string): Promise<void> => {
    await delay();
    const review = mockReviews.find(r => r.id === id);
    if (review) {
      if (!review.responses) review.responses = [];
      review.responses.push({
        id: `resp-${Date.now()}`,
        reviewId: id,
        userId: 'u3',
        userName: 'Tech Support',
        content: reply,
        createdAt: new Date().toISOString(),
      });
    }
  },
};
