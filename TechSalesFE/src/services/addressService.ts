import { Address } from '../models/ui_types/address';
import { mockAddresses } from '../data/mockData';

const delay = (ms = 30) => new Promise(resolve => setTimeout(resolve, ms));

export const addressService = {
  getAddresses: async (): Promise<Address[]> => {
    await delay();
    return [...mockAddresses];
  },

  createAddress: async (address: Omit<Address, 'id'>): Promise<void> => {
    await delay();
    const newAddress: Address = {
      id: `addr-${Date.now()}`,
      province: address.province,
      ward: address.ward,
      detail: address.detail,
      isDefault: mockAddresses.length === 0 || address.isDefault,
    };
    if (newAddress.isDefault) {
      mockAddresses.forEach(a => (a.isDefault = false));
    }
    mockAddresses.push(newAddress);
  },

  updateAddress: async (id: string, updates: Partial<Address>): Promise<void> => {
    await delay();
    const addr = mockAddresses.find(a => a.id === id);
    if (addr) {
      if (updates.province !== undefined) addr.province = updates.province;
      if (updates.ward !== undefined) addr.ward = updates.ward;
      if (updates.detail !== undefined) addr.detail = updates.detail;
      if (updates.isDefault) {
        mockAddresses.forEach(a => (a.isDefault = false));
        addr.isDefault = true;
      }
    }
  },

  deleteAddress: async (id: string): Promise<void> => {
    await delay();
    const index = mockAddresses.findIndex(a => a.id === id);
    if (index !== -1) {
      mockAddresses.splice(index, 1);
      if (mockAddresses.length > 0 && !mockAddresses.some(a => a.isDefault)) {
        mockAddresses[0].isDefault = true;
      }
    }
  },

  setDefaultAddress: async (id: string): Promise<void> => {
    await delay();
    mockAddresses.forEach(a => {
      a.isDefault = a.id === id;
    });
  },
};
