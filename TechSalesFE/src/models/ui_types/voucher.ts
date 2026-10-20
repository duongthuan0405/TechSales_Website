export enum VoucherType {
  FIXED = 'FIXED',
  PERCENTAGE = 'PERCENT', // Keep value 'PERCENT' to match BE but label is PERCENTAGE for FE
}

export interface Voucher {
  id: string;
  code: string;
  name?: string;
  description?: string;
  type: VoucherType;
  value: number;
  maxUsage: number;
  usedCount: number;
  minOrderAmount: number;
  startDate?: string;
  endDate?: string;
  isActive: boolean;
  createdAt: string;
}
