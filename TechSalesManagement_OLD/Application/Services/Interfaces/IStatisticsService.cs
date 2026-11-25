using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Repositories;

namespace TechSalesManagement.Application.Services.Interfaces;

public class RevenueChartDto
{
    public string Date { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}

public class CategoryDistributionDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class OrderStatusDistributionDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int CompletedOrders { get; set; }
    public decimal PendingRevenue { get; set; }
    public double TopProductSharePercentage { get; set; }
    public string TopProductCategoryName { get; set; } = string.Empty;
    public List<RevenueChartDto> RevenueTrend { get; set; } = new();
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    public List<OrderStatusDistributionDto> OrderStatusDistribution { get; set; } = new();
}

public interface IStatisticsService
{
    Task<List<RevenueChartDto>> GetDailyRevenueChartAsync(DateTimeOffset startDate, DateTimeOffset endDate);
    Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync();
    Task<ReportSummaryDto> GetReportSummaryAsync();
}
