export enum ReviewStatus {
  VISIBLE = 'VISIBLE',
  HIDDEN = 'HIDDEN',
  DELETED = 'DELETED'
}

export interface ReviewResponse {
  id: string;
  reviewId: string;
  userId: string;
  userName?: string;
  content: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Review {
  id: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  productId: string;
  rating: number; // 1 to 5
  comment: string;
  responses?: ReviewResponse[];
  status: ReviewStatus;
  createdAt: string;
  updatedAt?: string;
  productName?: string;
  productImage?: string;
  
  // Backward compatibility/Legacy field
  reply?: string;
}
