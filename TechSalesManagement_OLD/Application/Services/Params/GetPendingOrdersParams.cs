using System;

namespace TechSalesManagement.Application.Services.Params;

public class GetPendingOrdersParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
