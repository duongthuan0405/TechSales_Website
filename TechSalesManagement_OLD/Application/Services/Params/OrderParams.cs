using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Application.Services.Params;

public class PlaceOrderParams
{
    public Guid UserId { get; set; }
    public Dictionary<Guid, int> ProductsWithQuantity { get; set; } = new();
    public Guid ShippingAddressId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string? VoucherCode { get; set; }
}

public class GetOrderHistoryParams
{
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetOrderDetailsParams
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}

public class CancelOrderParams
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
}
