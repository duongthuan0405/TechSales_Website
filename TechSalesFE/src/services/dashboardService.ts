import { DashboardStats } from '../models/ui_types/dashboard';
import { orders, products, users, salesData, categoryData } from '../data/mockData';
import { OrderStatus } from '../models/ui_types/order';

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

const delay = (ms = 50) => new Promise(resolve => setTimeout(resolve, ms));

export const dashboardService = {
  getSalesStats: async (): Promise<DashboardStats> => {
    await delay();

    const nonCancelledOrders = orders.filter(o => o.status !== OrderStatus.CANCELLED);
    const totalRevenue = nonCancelledOrders.reduce((sum, o) => sum + o.totalAmount, 0);
    const totalOrders = orders.length;
    const avgOrderValue = totalOrders > 0 ? Math.round(totalRevenue / totalOrders) : 0;

    const activeCustomers = users.filter(u => u.role === 'Customer').length || 15;

    const pendingOrders = orders.filter(o => o.status === OrderStatus.PENDING).length;
    const shippingOrders = orders.filter(o => o.status === OrderStatus.SHIPPING).length;
    const deliveredOrders = orders.filter(o => o.status === OrderStatus.DELIVERED).length;

    const recentOrders = orders.slice(0, 5);

    return {
      totalRevenue,
      totalOrders,
      avgOrderValue,
      activeCustomers,
      revenueTrend: salesData,
      categoryDistribution: categoryData,
      recentOrders,
      pendingOrders,
      shippingOrders,
      deliveredOrders,
      averageProcessingTime: '2.4h',
    };
  },

  getReportSummary: async (): Promise<ReportSummaryDto> => {
    await delay();

    const deliveredOrders = orders.filter(o => o.status === OrderStatus.DELIVERED);
    const pendingOrders = orders.filter(o => o.status === OrderStatus.PENDING);

    const totalRevenue = orders
      .filter(o => o.status !== OrderStatus.CANCELLED)
      .reduce((sum, o) => sum + o.totalAmount, 0);

    const pendingRevenue = pendingOrders.reduce((sum, o) => sum + o.totalAmount, 0);

    const topSellingProducts: TopSellingProductDto[] = products.slice(0, 5).map((p, idx) => {
      const quantity = 45 - idx * 7;
      return {
        productId: p.id,
        name: p.name,
        quantity,
        revenue: p.price * quantity,
      };
    });

    const orderStatusDistribution: OrderStatusDistributionDto[] = [
      { status: 'Pending', count: orders.filter(o => o.status === OrderStatus.PENDING).length },
      { status: 'Approved', count: orders.filter(o => o.status === OrderStatus.APPROVED).length },
      { status: 'Shipping', count: orders.filter(o => o.status === OrderStatus.SHIPPING).length },
      { status: 'Delivered', count: orders.filter(o => o.status === OrderStatus.DELIVERED).length },
      { status: 'Cancelled', count: orders.filter(o => o.status === OrderStatus.CANCELLED).length },
    ];

    const revenueTrend = [
      { date: '2026-03-01', totalRevenue: 18500, orderCount: 42 },
      { date: '2026-03-08', totalRevenue: 24200, orderCount: 58 },
      { date: '2026-03-15', totalRevenue: 21400, orderCount: 49 },
      { date: '2026-03-22', totalRevenue: 31100, orderCount: 76 },
      { date: '2026-03-29', totalRevenue: 38900, orderCount: 92 },
    ];

    return {
      totalRevenue,
      completedOrders: deliveredOrders.length,
      pendingRevenue,
      topProductSharePercentage: 42,
      topProductCategoryName: 'Laptops',
      revenueTrend,
      topSellingProducts,
      orderStatusDistribution,
    };
  },
};
