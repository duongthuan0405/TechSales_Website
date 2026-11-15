using System;
using System.Collections.Generic;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

public class PlaceOrderRequestDto
{
    public Dictionary<Guid, int> productsWithQuantity { get; set; } = new();

    public Guid shippingAddressId { get; set; }

    public Guid paymentMethodId { get; set; }

    public string? voucherCode { get; set; }
}

public class OrderRepayRequestDto
{
    public Guid? paymentMethodId { get; set; }
}
