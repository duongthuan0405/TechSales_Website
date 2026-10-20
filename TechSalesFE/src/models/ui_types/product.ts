export enum ProductStatus {
  ACTIVE = 'ACTIVE',
  DISCONTINUED = 'DISCONTINUED'
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  status: ProductStatus;
  categoryId?: string;
  categoryName?: string; // For UI convenience
  imageUrl?: string; // From ProductImage (primary)
  images?: string[]; // All images
  stock: number; // From Inventory
  createdAt: string;
  updatedAt?: string;
  
  // UI extended fields (not directly in Product table but calculated or joined)
  rating?: number; // From Review table average
  reviewsCount?: number; // From Review table count
  brand?: string; // Note: Schema doesn't have brand explicitly, but might be in description or we keep for UI
}
