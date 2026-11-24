import { Product, ProductStatus } from '../models/ui_types/product';
import api, { ApiError, apiAssetUrl } from '../api/apiClient';

export interface ProductQueryParams {
  search?: string; categoryIds?: string; minPrice?: number; maxPrice?: number; brand?: string; brandId?: string;
  sortBy?: 'price_asc' | 'price_desc' | 'rating' | 'newest'; limit?: number; pageNumber?: number; pageSize?: number;
}
export interface ProductLookup { id: string; name: string; }
export interface AdminProductQuery {
  keyword?: string; categoryId?: string; brandId?: string; status?: ProductStatus;
  pageNumber?: number; pageSize?: number; minPrice?: number; maxPrice?: number;
  sortBy?: ProductQueryParams['sortBy'];
}
interface ProductRecord {
  id: string; name: string; description: string; price: number | null; status: ProductStatus;
  brandId: string; brandName?: string; categoryId: string; categoryName?: string;
  createdAt: string; updatedAt?: string;
  rating?: number; reviewsCount?: number;
  inventory?: { quantity: number; reservedQuantity: number };
  images: { id: string; imageUrl: string; isPrimary: boolean }[];
}
interface ProductPage {
  items: ProductRecord[]; totalCount: number; pageNumber: number; pageSize: number;
  brands: ProductLookup[]; categories: ProductLookup[];
}
export interface ProductWrite {
  name: string; description: string; price: number | null; brandId: string; categoryId: string;
  status?: ProductStatus; imageFiles?: File[]; primaryImageIndex?: number;
}
const mapProduct = (row: ProductRecord, lookups: { brands: ProductLookup[]; categories: ProductLookup[] }): Product => ({
  id: row.id, name: row.name, description: row.description, price: row.price ?? 0,
  priceIsUnset: row.price === null,
  status: row.status, brandId: row.brandId, categoryId: row.categoryId,
  brand: row.brandName ?? lookups.brands.find(brand => brand.id === row.brandId)?.name,
  categoryName: row.categoryName ?? lookups.categories.find(category => category.id === row.categoryId)?.name,
  createdAt: row.createdAt, updatedAt: row.updatedAt,
  stock: row.inventory?.quantity,
  rating: row.rating ?? 0, reviewsCount: row.reviewsCount ?? 0,
  imageUrl: apiAssetUrl(row.images.find(image => image.isPrimary)?.imageUrl ?? row.images[0]?.imageUrl),
  images: row.images.map(image => apiAssetUrl(image.imageUrl)!),
  imageDetails: row.images.map(image => ({ ...image, imageUrl: apiAssetUrl(image.imageUrl)! })),
});
const query = (params: Record<string, unknown>) => {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '' && value !== 'all') search.set(key, String(value));
  }
  return search.toString();
};
const canReadAdmin = () => {
  try {
    const user = JSON.parse(localStorage.getItem('tech_sales_user') || 'null');
    return [user?.role, ...(user?.roles || [])].some(role => ['Business Admin', 'Technical Admin', 'Staff'].includes(role));
  } catch { return false; }
};
const lookups = async () => {
  const page = await api.get<ProductPage>('/api/products?pageSize=1');
  return { brands: page.brands, categories: page.categories };
};

export const productService = {
  getLookups: lookups,
  getProductPage: async (params: ProductQueryParams = {}) => {
    const page = await api.get<ProductPage>(`/api/products?${query({
      pageNumber: params.pageNumber ?? 1, pageSize: params.pageSize ?? 20,
      keyword: params.search, categoryId: params.categoryIds, brandId: params.brandId,
      minPrice: params.minPrice, maxPrice: params.maxPrice, sortBy: params.sortBy,
    })}`);
    return { ...page, items: page.items.map(row => mapProduct(row, page)) };
  },
  getProducts: async (params?: ProductQueryParams): Promise<Product[]> => {
    let pageNumber = 1;
    let result: Product[] = [];
    while (true) {
      const page = await api.get<ProductPage>(`/api/products?${query({ pageNumber, pageSize: params?.limit ? Math.min(params.limit, 100) : 100, keyword: params?.search, categoryId: params?.categoryIds, brandId: params?.brandId,
        minPrice: params?.minPrice, maxPrice: params?.maxPrice, sortBy: params?.sortBy })}`);
      result.push(...page.items.map(row => mapProduct(row, page)));
      if (page.items.length === 0 || pageNumber * page.pageSize >= page.totalCount || (params?.limit && result.length >= params.limit)) break;
      pageNumber += 1;
    }
    if (params?.limit) result = result.slice(0, params.limit);
    return result;
  },
  getAdminProducts: async (params?: AdminProductQuery) => {
    const page = await api.get<ProductPage>(`/api/admin/products?${query(params || {})}`);
    return { ...page, items: page.items.map(row => mapProduct(row, page)) };
  },
  getProductById: async (id: string): Promise<Product> => {
    const prefix = canReadAdmin() ? '/api/admin/products' : '/api/products';
    const page = await api.get<ProductPage>(`${prefix}/${encodeURIComponent(id)}`);
    return mapProduct(page.items[0], page);
  },
  getAllBrands: async (): Promise<string[]> => (await lookups()).brands.map(brand => brand.name),
  createProduct: async (data: ProductWrite): Promise<Product> => {
    let row: ProductRecord;
    if (data.imageFiles?.length) {
      const body = new FormData();
      body.set('Name', data.name);
      body.set('Description', data.description);
      if (data.price !== null) body.set('Price', String(data.price));
      body.set('BrandId', data.brandId);
      body.set('CategoryId', data.categoryId);
      if (data.primaryImageIndex !== undefined) body.set('PrimaryImageIndex', String(data.primaryImageIndex));
      data.imageFiles.forEach(file => body.append('Images', file));
      row = await api.post<ProductRecord>('/api/admin/products/with-images', body);
    } else {
      row = await api.post<ProductRecord>('/api/admin/products', data);
    }
    return mapProduct(row, await lookups());
  },
  updateProduct: async (id: string, data: Partial<Omit<Product, 'price'>> & { price?: number | null; retainedImageIds?: string[]; imageFiles?: File[]; primaryImageIndex?: number }): Promise<Product> => {
    const current = await productService.getProductById(id);
    const body = { name: data.name ?? current.name, description: data.description ?? current.description,
      price: data.price !== undefined ? data.price : current.priceIsUnset ? null : current.price,
      brandId: data.brandId ?? current.brandId,
      categoryId: data.categoryId ?? current.categoryId, status: data.status ?? current.status };
    const row = await api.put<ProductRecord>(`/api/admin/products/${encodeURIComponent(id)}`, body);
    if (data.retainedImageIds !== undefined) {
      const images = new FormData();
      data.retainedImageIds.forEach(imageId => images.append('RetainedImageIds', imageId));
      data.imageFiles?.forEach(file => images.append('Images', file));
      images.set('PrimaryImageIndex', data.primaryImageIndex === undefined ? '' : String(data.primaryImageIndex));
      await api.put(`/api/admin/products/${encodeURIComponent(id)}/images`, images);
      return productService.getProductById(id);
    }
    return mapProduct(row, await lookups());
  },
  toggleProductDiscontinue: async (id: string): Promise<Product> => {
    const current = await productService.getProductById(id);
    return productService.updateProduct(id, { status: current.status === ProductStatus.ACTIVE ? ProductStatus.DISCONTINUED : ProductStatus.ACTIVE });
  },
  deleteProduct: async (id: string): Promise<void> => {
    await productService.updateProduct(id, { status: ProductStatus.DISCONTINUED });
  },
  updateInventory: async (id: string, serialNumbers: string[]): Promise<void> => {
    await api.post(`/api/inventory/${encodeURIComponent(id)}/receive`, { serialNumbers });
  },
};
