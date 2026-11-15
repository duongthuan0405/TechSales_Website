using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Application.Services.Interfaces;

namespace TechSalesManagement.Infrastructure.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly TechSalesDbContext _dbContext;

    public StatisticsRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RevenueDataPoint>> GetDailyRevenueAsync(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        // Aggregate revenue from recognized sales: must be DELIVERED, and if not Cash, must be paid successfully
        var data = await _dbContext.Orders
            .Where(o => o.status == OrderStatus.DELIVERED && (
                o.payments.Any(p => p.payment_method.type == PaymentMethodType.CASH) || 
                o.payments.Any(p => p.payment_method.type != PaymentMethodType.CASH && p.status == PaymentStatus.SUCCESS)
            ))
            .Where(o => o.created_at >= startDate && o.created_at <= endDate)
            .GroupBy(o => o.created_at.Date)
            .Select(g => new RevenueDataPoint
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.total_amount),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        return data;
    }

    public async Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync()
    {
        // Category sales from recognized sales: must be DELIVERED, and if not Cash, must be paid successfully
        var data = await _dbContext.OrderItems
            .Where(oi => oi.order.status == OrderStatus.DELIVERED && (
                oi.order.payments.Any(p => p.payment_method.type == PaymentMethodType.CASH) || 
                oi.order.payments.Any(p => p.payment_method.type != PaymentMethodType.CASH && p.status == PaymentStatus.SUCCESS)
            ))
            .GroupBy(oi => oi.product.category.name)
            .Select(g => new CategoryDistributionDto
            {
                Name = g.Key,
                Value = g.Sum(oi => oi.price * oi.quantity)
            })
            .ToListAsync();

        return data;
    }

    public async Task<ReportSummaryDto> GetReportSummaryAsync()
    {
        // 1. Total revenue from recognized sales: must be DELIVERED, and if not Cash, must be paid successfully
        var totalRevenue = await _dbContext.Orders
            .Where(o => o.status == OrderStatus.DELIVERED && (
                o.payments.Any(p => p.payment_method.type == PaymentMethodType.CASH) || 
                o.payments.Any(p => p.payment_method.type != PaymentMethodType.CASH && p.status == PaymentStatus.SUCCESS)
            ))
            .SumAsync(o => (decimal?)o.total_amount) ?? 0;

        // 2. Completed orders count
        var completedOrders = await _dbContext.Orders
            .CountAsync(o => o.status == OrderStatus.DELIVERED);

        // 3. Pending revenue (sum of total_amount of pending or approved orders)
        var pendingRevenue = await _dbContext.Orders
            .Where(o => o.status == OrderStatus.PENDING || o.status == OrderStatus.APPROVED)
            .SumAsync(o => (decimal?)o.total_amount) ?? 0;

        // 4. Order status distribution
        var rawStatuses = await _dbContext.Orders
            .GroupBy(o => o.status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var orderStatusDistribution = rawStatuses.Select(s => new OrderStatusDistributionDto
        {
            Status = s.Status.ToString(),
            Count = s.Count
        }).ToList();

        // 5. Top selling products
        var rawProductSales = await _dbContext.OrderItems
            .Where(oi => oi.order.status != OrderStatus.CANCELLED)
            .GroupBy(oi => new { oi.product_id, oi.product.name })
            .Select(g => new
            {
                ProductId = g.Key.product_id,
                Name = g.Key.name,
                Quantity = g.Sum(oi => oi.quantity),
                Revenue = g.Sum(oi => oi.price * oi.quantity)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToListAsync();

        var topSellingProducts = rawProductSales.Select(p => new TopSellingProductDto
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Quantity = p.Quantity,
            Revenue = p.Revenue
        }).ToList();

        // 6. Top product share percentage and category name
        double topProductSharePercentage = 0;
        string topProductCategoryName = "Hardware";

        if (topSellingProducts.Count > 0 && totalRevenue > 0)
        {
            var topProd = topSellingProducts[0];
            topProductSharePercentage = (double)((topProd.Revenue / totalRevenue) * 100);

            // Get product with category info
            var topProductEntity = await _dbContext.Products
                .Include(p => p.category)
                .FirstOrDefaultAsync(p => p.id == topProd.ProductId);
            if (topProductEntity?.category != null)
            {
                topProductCategoryName = topProductEntity.category.name;
            }
        }

        // 7. 7-day revenue trend from recognized sales: must be DELIVERED, and if not Cash, must be paid successfully
        var endDate = DateTimeOffset.UtcNow;
        var startDate = endDate.AddDays(-6);
        
        var rawTrend = await _dbContext.Orders
            .Where(o => o.status == OrderStatus.DELIVERED && (
                o.payments.Any(p => p.payment_method.type == PaymentMethodType.CASH) || 
                o.payments.Any(p => p.payment_method.type != PaymentMethodType.CASH && p.status == PaymentStatus.SUCCESS)
            ))
            .Where(o => o.created_at >= startDate && o.created_at <= endDate)
            .GroupBy(o => o.created_at.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.total_amount),
                OrderCount = g.Count()
            })
            .ToListAsync();

        var revenueTrend = new List<RevenueChartDto>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var point = rawTrend.FirstOrDefault(d => d.Date == date);
            revenueTrend.Add(new RevenueChartDto
            {
                Date = date.ToString("yyyy-MM-dd"),
                TotalRevenue = point?.Revenue ?? 0,
                OrderCount = point?.OrderCount ?? 0
            });
        }

        return new ReportSummaryDto
        {
            TotalRevenue = totalRevenue,
            CompletedOrders = completedOrders,
            PendingRevenue = pendingRevenue,
            TopProductSharePercentage = Math.Round(topProductSharePercentage, 1),
            TopProductCategoryName = topProductCategoryName,
            TopSellingProducts = topSellingProducts,
            OrderStatusDistribution = orderStatusDistribution,
            RevenueTrend = revenueTrend
        };
    }
}
