import api from '../api/apiClient';

export interface ShippingFee { provinceCode: string; fee: number; }
export const shippingService = {
  getFee: (provinceCode: string): Promise<ShippingFee> => api.get(`/api/shipping/provinces/${encodeURIComponent(provinceCode)}/fee`),
  setFee: (provinceCode: string, fee: number): Promise<ShippingFee> => api.put(`/api/shipping/provinces/${encodeURIComponent(provinceCode)}/fee`, { fee }),
};
