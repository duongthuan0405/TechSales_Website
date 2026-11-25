using TechSalesManagement.Application.Services.Interfaces;

namespace TechSalesManagement.Application.Repositories;

public class RevenueDataPoint
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public interface IStatisticsRepository
{
    Task<List<RevenueDataPoint>> GetDailyRevenueAsync(DateTimeOffset startDate, DateTimeOffset endDate);
    Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync();
    Task<ReportSummaryDto> GetReportSummaryAsync();
}
