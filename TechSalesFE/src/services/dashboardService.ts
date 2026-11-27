import { unsupportedFeature } from './unsupportedFeature';
import { DashboardStats } from '../models/ui_types/dashboard';



export interface TopSellingProductDto {
  productId: string;
  name: string;
  quantity: number;
  revenue: number;
}

export interface OrderStatusDistributionDto {
  status: string;
  count: number;
}

export interface ReportSummaryDto {
  totalRevenue: number;
  completedOrders: number;
  pendingRevenue: number;
  topProductSharePercentage: number;
  topProductCategoryName: string;
  revenueTrend: { date: string; totalRevenue: number; orderCount: number }[];
  topSellingProducts: TopSellingProductDto[];
  orderStatusDistribution: OrderStatusDistributionDto[];
}

export const dashboardService = {
  getSalesStats: async (): Promise<DashboardStats> => { return unsupportedFeature("thống kê và báo cáo"); },

  getReportSummary: async (): Promise<ReportSummaryDto> => { return unsupportedFeature("thống kê và báo cáo"); },
};
