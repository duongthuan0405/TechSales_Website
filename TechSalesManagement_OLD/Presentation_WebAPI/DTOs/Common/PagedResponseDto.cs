using System;
using System.Collections.Generic;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.Common;

public class PagedResponseDto<T>
{
    public List<T> items { get; set; } = new();
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public int totalCount { get; set; }
    public int totalPages => pageSize == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
}
