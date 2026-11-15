using System;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class OrderSearchRequestDto
{
    public string? orderCode { get; set; }
    public string? customerName { get; set; }
    public string? phoneNumber { get; set; }
    public DateTimeOffset? fromDate { get; set; }
    public DateTimeOffset? toDate { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 20;
}
