using System;

namespace TechSalesManagement.Application.Services.Params;

public class ApproveOrderParams
{
    public Guid OrderId { get; set; }
    public Guid StaffId { get; set; }
}
