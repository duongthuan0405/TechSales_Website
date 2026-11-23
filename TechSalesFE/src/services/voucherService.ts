import { Voucher, VoucherType } from '../models/ui_types/voucher';
import api, { ApiError } from '../api/apiClient';
import { PagedResponse } from './userService';

interface VoucherRecord extends Omit<Voucher, 'value' | 'maxUsage' | 'minOrderAmount' | 'createdAt'> {
  value: number | null; maxUsage: number | null; minOrderAmount: number | null; createdAt: string | null;
}
interface VoucherPage { items: VoucherRecord[]; totalCount: number; pageNumber: number; pageSize: number; }
const mapVoucher = (row: VoucherRecord): Voucher => ({ ...row, value: row.value ?? 0,
  maxUsage: row.maxUsage ?? Number.MAX_SAFE_INTEGER, minOrderAmount: row.minOrderAmount ?? 0, createdAt: row.createdAt ?? '' });

export const voucherService = {
  getVouchers: async (page = 1, pageSize = 20): Promise<PagedResponse<Voucher>> => {
    const data = await api.get<VoucherPage>(`/api/admin/vouchers?pageNumber=${page}&pageSize=${pageSize}`);
    return { ...data, items: data.items.map(mapVoucher), totalPages: Math.ceil(data.totalCount / data.pageSize) };
  },
  createVoucher: async (data: { code: string; type: VoucherType; value: number; maxUsage: number;
    minOrderAmount: number; startDate?: string; endDate?: string }): Promise<Voucher> => {
    return mapVoucher(await api.post<VoucherRecord>('/api/admin/vouchers', { ...data,
      startDate: data.startDate ? new Date(data.startDate).toISOString() : null,
      endDate: data.endDate ? new Date(data.endDate).toISOString() : null }));
  },
  deleteVoucher: async (id: string): Promise<void> => { await api.delete(`/api/admin/vouchers/${encodeURIComponent(id)}`); },
  getAvailableVouchers: async (): Promise<Voucher[]> => {
    const result: Voucher[] = [];
    for (let page = 1; ; page++) {
      const data = await api.get<VoucherPage>(`/api/vouchers?pageNumber=${page}&pageSize=100`);
      result.push(...data.items.map(mapVoucher));
      if (!data.items.length || page * data.pageSize >= data.totalCount) return result;
    }
  },
  // Preview only. Final validation and consumption must be performed by the future Order API.
  validateVoucher: async (code: string, orderAmount: number): Promise<Voucher> => {
    const voucher = (await voucherService.getAvailableVouchers()).find(item => item.code.toUpperCase() === code.trim().toUpperCase());
    const now = Date.now();
    if (!voucher || !voucher.isActive || voucher.usedCount >= voucher.maxUsage
      || (voucher.startDate && Date.parse(voucher.startDate) > now)
      || (voucher.endDate && Date.parse(voucher.endDate) < now)) throw new ApiError('Mã giảm giá không khả dụng.', 400);
    if (orderAmount < voucher.minOrderAmount) throw new ApiError('Đơn hàng chưa đạt giá trị tối thiểu của mã giảm giá.', 400);
    return voucher;
  },
};
