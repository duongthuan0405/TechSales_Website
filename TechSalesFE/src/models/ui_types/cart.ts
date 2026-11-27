export interface CartItem {
  cartId: string;
  productId: string;
  quantity: number;
  createdAt?: string;
  updatedAt?: string;
  
  // UI convenience
  productName?: string;
  price?: number;
  imageUrl?: string;
  unavailable?: boolean;
}

export interface Cart {
  id: string;
  userId: string;
  createdAt: string;
  items: CartItem[];
}
