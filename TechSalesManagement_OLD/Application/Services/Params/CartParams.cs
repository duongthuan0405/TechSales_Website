using System;

namespace TechSalesManagement.Application.Services.Params;

public class AddToCartParams
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemParams
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class RemoveCartItemParams
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
}

public class GetCartParams
{
    public Guid UserId { get; set; }
}
