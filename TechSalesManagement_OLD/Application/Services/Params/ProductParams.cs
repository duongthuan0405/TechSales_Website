using System;
using System.Collections.Generic;
using TechSalesManagement.Application.Enums;

namespace TechSalesManagement.Application.Services.Params;

public class SearchProductParams
{
    public string? Keyword { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public SortOrder? SortOrder { get; set; }
}

public class GetProductDetailsParams
{
    public Guid ProductId { get; set; }
}
