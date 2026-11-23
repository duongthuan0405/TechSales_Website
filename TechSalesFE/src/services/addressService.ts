import { Address } from '../models/ui_types/address';
import api, { ApiError } from '../api/apiClient';

interface AddressRecord { id: string; provinceCode: string; provinceName: string | null;
  wardCode: string; wardName: string | null; detail: string; isDefault: boolean; }
export interface Location { code: string; name: string | null; }
export const addressService = {
  getProvinces: async (): Promise<Location[]> => (await api.get<{ items: Location[] }>('/api/shipping/provinces')).items,
  getWards: async (provinceCode: string): Promise<Location[]> =>
    (await api.get<{ items: Location[] }>(`/api/shipping/provinces/${encodeURIComponent(provinceCode)}/wards`)).items,
  getAddresses: async (): Promise<Address[]> => {
    const data = await api.get<{ items: AddressRecord[] }>('/api/shipping-address');
    return data.items.map(row => ({ id: row.id, province: row.provinceName ?? row.provinceCode,
      ward: row.wardName ?? row.wardCode, provinceCode: row.provinceCode, wardCode: row.wardCode,
      detail: row.detail, isDefault: row.isDefault }));
  },
  createAddress: async (address: Omit<Address, 'id'>): Promise<void> => {
    if (!address.wardCode) throw new ApiError('Vui lòng chọn tỉnh/thành và phường/xã.', 400);
    const created = await api.post<AddressRecord>('/api/shipping-address', { wardCode: address.wardCode, detail: address.detail });
    if (address.isDefault && !created.isDefault) await addressService.setDefaultAddress(created.id);
  },
  updateAddress: async (id: string, updates: Partial<Address>): Promise<void> => {
    const current = (await addressService.getAddresses()).find(address => address.id === id);
    if (!current) throw new ApiError('Không tìm thấy địa chỉ.', 404);
    await api.put(`/api/shipping-address/${encodeURIComponent(id)}`, {
      wardCode: updates.wardCode ?? current.wardCode, detail: updates.detail ?? current.detail });
    if (updates.isDefault && !current.isDefault) await addressService.setDefaultAddress(id);
  },
  deleteAddress: async (id: string): Promise<void> => { await api.delete(`/api/shipping-address/${encodeURIComponent(id)}`); },
  setDefaultAddress: async (id: string): Promise<void> => { await api.put(`/api/shipping-address/${encodeURIComponent(id)}/default`); },
};
