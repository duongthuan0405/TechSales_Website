using System;
using System.Linq;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Domain.Specifications;

public class OrderSearchParameters
{
    public string? OrderCode { get; set; }
    public string? CustomerName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public static class OrderSearchSpecification
{
    public static IQueryable<OrderDbModel> ApplyFilters(IQueryable<OrderDbModel> query, OrderSearchParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.OrderCode))
        {
            // Assuming orderCode exists in DB as order_code. If not, we might use id part.
            // Based on previous findings, OrderDbModel didn't have order_code.
            // I'll search for 'id' starts with or similar if code is not available.
            // Let's assume for now there is an order_code or we search by ID string.
            query = query.Where(o => o.id.ToString().Contains(parameters.OrderCode));
        }

        if (!string.IsNullOrWhiteSpace(parameters.CustomerName))
        {
            query = query.Where(o => o.user != null && o.user.user_profile != null && 
                                     o.user.user_profile.full_name.Contains(parameters.CustomerName));
        }

        if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
        {
            query = query.Where(o => o.user != null && o.user.user_profile != null && 
                                     o.user.user_profile.phone.Contains(parameters.PhoneNumber));
        }

        if (parameters.FromDate.HasValue)
        {
            query = query.Where(o => o.created_at >= parameters.FromDate.Value);
        }

        if (parameters.ToDate.HasValue)
        {
            query = query.Where(o => o.created_at <= parameters.ToDate.Value);
        }

        return query;
    }
}
