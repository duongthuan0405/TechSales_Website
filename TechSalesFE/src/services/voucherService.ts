import { Voucher, VoucherType } from '../models/ui_types/voucher';
import { vouchers } from '../data/mockData';
import { PagedResponse } from './userService';

const delay = (ms = 40) => new Promise(resolve => setTimeout(resolve, ms));

export const voucherService = {
  getVouchers: async (page = 1, pageSize = 20): Promise<PagedResponse<Voucher>> => {
    await delay();
    const totalCount = vouchers.length;
    const startIndex = (page - 1) * pageSize;
    const items = vouchers.slice(startIndex, startIndex + pageSize);

    return {
      items,
      pageNumber: page,
      pageSize,
      totalCount,
      totalPages: Math.ceil(totalCount / pageSize) || 1,
    };
  },

  createVoucher: async (data: {
    code: string;
    type: VoucherType;
    value: number;
    maxUsage: number;
    minOrderAmount: number;
    startDate?: string;
    endDate?: string;
  }): Promise<Voucher> => {
    await delay();
    const newVoucher: Voucher = {
      id: `v${Date.now()}`,
      code: data.code.toUpperCase().trim(),
      name: data.code.toUpperCase().trim(),
      type: data.type,
      value: Number(data.value),
      maxUsage: Number(data.maxUsage) || 100,
      usedCount: 0,
      minOrderAmount: Number(data.minOrderAmount) || 0,
      startDate: data.startDate || new Date().toISOString(),
      endDate: data.endDate || new Date(Date.now() + 60 * 86400000).toISOString(),
      isActive: true,
      createdAt: new Date().toISOString(),
    };

    vouchers.unshift(newVoucher);
    return newVoucher;
  },

  deleteVoucher: async (id: string): Promise<void> => {
    await delay();
    const index = vouchers.findIndex(v => v.id === id);
    if (index !== -1) {
      vouchers.splice(index, 1);
    }
  },

  getAvailableVouchers: async (): Promise<Voucher[]> => {
    await delay();
    return vouchers.filter(v => v.isActive);
  },

  validateVoucher: async (code: string, orderAmount: number): Promise<Voucher> => {
    await delay();
    const cleanCode = code.trim().toUpperCase();
    const voucher = vouchers.find(v => v.code.toUpperCase() === cleanCode && v.isActive);

    if (!voucher) {
      throw new Error(`Invalid or expired voucher code: ${code}`);
    }

    if (orderAmount < voucher.minOrderAmount) {
      throw new Error(`Minimum order of $${voucher.minOrderAmount} required to use this voucher`);
    }

    return voucher;
  },
};
