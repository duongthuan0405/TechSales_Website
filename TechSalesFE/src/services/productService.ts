import { Product, ProductStatus } from '../models/ui_types/product';
import { products, categories } from '../data/mockData';

export interface ProductQueryParams {
  search?: string;
  categoryIds?: string;
  minPrice?: number;
  maxPrice?: number;
  brand?: string;
  sortBy?: 'price_asc' | 'price_desc' | 'rating' | 'newest';
  limit?: number;
}

const delay = (ms = 50) => new Promise(resolve => setTimeout(resolve, ms));

export const productService = {
  getProducts: async (params?: ProductQueryParams): Promise<Product[]> => {
    await delay();
    let result = [...products];

    // Status filter: only active products in customer store
    result = result.filter(p => p.status === ProductStatus.ACTIVE);

    if (params?.search) {
      const q = params.search.toLowerCase().trim();
      result = result.filter(
        p =>
          p.name.toLowerCase().includes(q) ||
          p.description.toLowerCase().includes(q) ||
          p.brand?.toLowerCase().includes(q) ||
          p.categoryName?.toLowerCase().includes(q)
      );
    }

    if (params?.categoryIds && params.categoryIds !== 'all') {
      const catId = params.categoryIds;
      result = result.filter(
        p =>
          p.categoryId === catId ||
          p.categoryName?.toLowerCase() === catId.toLowerCase()
      );
    }

    if (params?.brand && params.brand !== 'all') {
      const brandQ = params.brand.toLowerCase();
      result = result.filter(p => p.brand?.toLowerCase() === brandQ);
    }

    if (params?.minPrice !== undefined && params.minPrice > 0) {
      result = result.filter(p => p.price >= params.minPrice!);
    }

    if (params?.maxPrice !== undefined && params.maxPrice > 0) {
      result = result.filter(p => p.price <= params.maxPrice!);
    }

    if (params?.sortBy) {
      switch (params.sortBy) {
        case 'price_asc':
          result.sort((a, b) => a.price - b.price);
          break;
        case 'price_desc':
          result.sort((a, b) => b.price - a.price);
          break;
        case 'rating':
          result.sort((a, b) => (b.rating || 0) - (a.rating || 0));
          break;
        case 'newest':
          result.sort((a, b) => new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime());
          break;
      }
    }

    if (params?.limit && params.limit > 0) {
      result = result.slice(0, params.limit);
    }

    return result;
  },

  getAdminProducts: async (params?: {
    keyword?: string;
    categoryId?: string;
    status?: ProductStatus;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ items: Product[]; totalCount: number }> => {
    await delay();
    let result = [...products];

    if (params?.keyword) {
      const q = params.keyword.toLowerCase().trim();
      result = result.filter(
        p =>
          p.name.toLowerCase().includes(q) ||
          p.description.toLowerCase().includes(q) ||
          p.brand?.toLowerCase().includes(q)
      );
    }

    if (params?.categoryId && params.categoryId !== 'all') {
      result = result.filter(
        p =>
          p.categoryId === params.categoryId ||
          p.categoryName?.toLowerCase() === params.categoryId?.toLowerCase()
      );
    }

    if (params?.status) {
      result = result.filter(p => p.status === params.status);
    }

    const totalCount = result.length;
    const pageNumber = params?.pageNumber || 1;
    const pageSize = params?.pageSize || 20;
    const startIndex = (pageNumber - 1) * pageSize;
    const items = result.slice(startIndex, startIndex + pageSize);

    return {
      items,
      totalCount,
    };
  },

  getProductById: async (id: string): Promise<Product> => {
    await delay();
    const product = products.find(p => p.id === id);
    if (!product) {
      throw new Error(`Product with ID ${id} not found`);
    }
    return { ...product };
  },

  getAllBrands: async (): Promise<string[]> => {
    await delay();
    const brands = products.map(p => p.brand).filter(Boolean) as string[];
    return Array.from(new Set(brands));
  },

  createProduct: async (productData: any): Promise<Product> => {
    await delay();
    const cat = categories.find(c => c.id === productData.categoryId);
    const id = `p${Date.now()}`;

    // If file uploaded, create a data URL or use default tech placeholder
    let imageUrl = 'https://images.unsplash.com/photo-1526738549149-8e07eca6c147?w=400';
    if (productData.imageUrl) {
      imageUrl = productData.imageUrl;
    } else if (productData.imageFiles && productData.imageFiles.length > 0) {
      imageUrl = URL.createObjectURL(productData.imageFiles[0]);
    }

    const newProduct: Product = {
      id,
      name: productData.name,
      description: productData.description || '',
      price: Number(productData.price) || 0,
      brand: productData.brand || 'TechSales',
      categoryId: productData.categoryId,
      categoryName: cat ? cat.name : productData.categoryName || 'General',
      stock: Number(productData.initialStock || productData.stock) || 0,
      status: ProductStatus.ACTIVE,
      rating: 5.0,
      reviewsCount: 0,
      imageUrl,
      images: [imageUrl],
      createdAt: new Date().toISOString(),
    };

    products.unshift(newProduct);
    if (cat) {
      cat.productCount = (cat.productCount || 0) + 1;
    }

    return newProduct;
  },

  updateProduct: async (id: string, productData: Partial<Product>): Promise<Product> => {
    await delay();
    const index = products.findIndex(p => p.id === id);
    if (index === -1) {
      throw new Error(`Product ${id} not found`);
    }

    const cat = productData.categoryId ? categories.find(c => c.id === productData.categoryId) : undefined;

    products[index] = {
      ...products[index],
      ...productData,
      categoryName: cat ? cat.name : (productData.categoryName || products[index].categoryName),
      updatedAt: new Date().toISOString(),
    };

    return { ...products[index] };
  },

  deleteProduct: async (id: string): Promise<void> => {
    await delay();
    const index = products.findIndex(p => p.id === id);
    if (index !== -1) {
      products[index].status = ProductStatus.DISCONTINUED;
    }
  },

  toggleProductDiscontinue: async (id: string): Promise<Product> => {
    await delay();
    const index = products.findIndex(p => p.id === id);
    if (index === -1) {
      throw new Error(`Product ${id} not found`);
    }

    const nextStatus =
      products[index].status === ProductStatus.ACTIVE
        ? ProductStatus.DISCONTINUED
        : ProductStatus.ACTIVE;

    products[index].status = nextStatus;
    return { ...products[index] };
  },

  updateInventory: async (id: string, value: number, type: 'ADD' | 'SET'): Promise<void> => {
    await delay();
    const product = products.find(p => p.id === id);
    if (product) {
      if (type === 'ADD') {
        product.stock = Math.max(0, (product.stock || 0) + value);
      } else {
        product.stock = Math.max(0, value);
      }
    }
  },
};
