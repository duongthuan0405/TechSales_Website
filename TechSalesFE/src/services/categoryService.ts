import { Category } from '../models/ui_types/category';
import api, { ApiError } from '../api/apiClient';

export const categoryService = {
  getCategories: (): Promise<Category[]> => api.get('/api/categories'),
  createCategory: (data: Omit<Category, 'id'>): Promise<Category> =>
    api.post('/api/admin/categories', { name: data.name }),
  updateCategory: (id: string, data: Partial<Category>): Promise<Category> =>
    api.put(`/api/admin/categories/${encodeURIComponent(id)}`, { name: data.name }),
  deleteCategory: async (_id: string, _replacementId?: string): Promise<void> => {
    throw new ApiError('Chưa hỗ trợ xóa hoặc chuyển sản phẩm sang danh mục thay thế.', 501);
  },
};
