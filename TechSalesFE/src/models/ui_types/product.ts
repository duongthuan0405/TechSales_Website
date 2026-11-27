export enum ProductStatus {
  ACTIVE = 'ACTIVE',
  DISCONTINUED = 'DISCONTINUED'
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  priceIsUnset?: boolean;
  status: ProductStatus;
  categoryId?: string;
  categoryName?: string; // For UI convenience
  imageUrl?: string; // From ProductImage (primary)
  images?: string[]; // All images
  imageDetails?: { id: string; imageUrl: string; isPrimary: boolean }[];
  stock?: number; // Available when Inventory has a read API.
  createdAt: string;
  updatedAt?: string;
  
  // UI extended fields (not directly in Product table but calculated or joined)
  rating?: number; // From Review table average
  reviewsCount?: number; // From Review table count
  brand?: string; // Display name resolved from BrandId.
  brandId?: string;
}
