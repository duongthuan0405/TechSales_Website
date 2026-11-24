import api from '../api/apiClient';

export interface Brand { id: string; name: string; }
export interface BrandQuery { pageNumber: number; pageSize: number; keyword?: string; }
export const brandService = {
  getAdminBrands: (params: BrandQuery): Promise<{ items: Brand[]; totalCount: number }> => {
    const query = new URLSearchParams({ pageNumber: String(params.pageNumber), pageSize: String(params.pageSize) });
    if (params.keyword?.trim()) query.set('keyword', params.keyword.trim());
    return api.get(`/api/admin/brands?${query}`);
  },
  createBrand: (data: { name: string }): Promise<Brand> => api.post('/api/admin/brands', data),
  updateBrand: (id: string, data: { name: string }): Promise<Brand> => api.put(`/api/admin/brands/${encodeURIComponent(id)}`, data),
};
