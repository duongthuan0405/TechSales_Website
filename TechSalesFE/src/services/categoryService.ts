import { Category } from '../models/ui_types/category';
import api, { ApiError } from '../api/apiClient';

export interface CategoryPage { items: Category[]; totalCount: number; pageNumber: number; pageSize: number; }
export interface CategoryQuery { pageNumber?: number; pageSize?: number; keyword?: string; }

export const categoryService = {
  getCategories: (): Promise<Category[]> => api.get('/api/categories'),
  getAdminCategories: (params: CategoryQuery = {}): Promise<CategoryPage> => {
    const query = new URLSearchParams({ pageNumber: String(params.pageNumber ?? 1), pageSize: String(params.pageSize ?? 20) });
    if (params.keyword?.trim()) query.set('keyword', params.keyword.trim());
    return api.get(`/api/admin/categories?${query}`);
  },
  createCategory: (data: Omit<Category, 'id'>): Promise<Category> =>
    api.post('/api/admin/categories', { name: data.name }),
  updateCategory: (id: string, data: Partial<Category>): Promise<Category> =>
    api.put(`/api/admin/categories/${encodeURIComponent(id)}`, { name: data.name }),
  deleteCategory: async (_id: string, _replacementId?: string): Promise<void> => {
    throw new ApiError('Chưa hỗ trợ xóa hoặc chuyển sản phẩm sang danh mục thay thế.', 501);
  },
};
