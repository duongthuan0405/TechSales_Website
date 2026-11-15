using System;

namespace TechSalesManagement.Application.Services.Params;

public class GetUserByIdParams
{
    public required Guid UserId { get; set; }
}
