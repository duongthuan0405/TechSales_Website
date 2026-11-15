using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

public class OrderResponseDto
{
    public Guid id { get; set; }
    public OrderStatus status { get; set; }
    public decimal totalAmount { get; set; }
    public DateTimeOffset createdAt { get; set; }
    public string paymentMethodName { get; set; } = string.Empty;
    public bool? isPaymentFailed { get; set; }
    public string? checkoutUrl { get; set; }
}

public class OrderDetailResponseDto
{
    public Guid id { get; set; }
    public OrderStatus status { get; set; }
    public decimal totalProductAmount { get; set; }
    public decimal shippingFee { get; set; }
    public decimal discountAmount { get; set; }
    public decimal totalAmount { get; set; }
    public string shippingAddressSnapshot { get; set; } = string.Empty;
    public DateTimeOffset createdAt { get; set; }
    public DateTimeOffset? approvedAt { get; set; }
    public DateTimeOffset? shippedAt { get; set; }
    public DateTimeOffset? deliveredAt { get; set; }
    public string paymentMethodName { get; set; } = string.Empty;
    public bool? isPaymentFailed { get; set; }
    public List<OrderItemResponseDto> items { get; set; } = new();
    public List<PaymentResponseDto> payments { get; set; } = new();
}

public class OrderItemResponseDto
{
    public Guid productId { get; set; }
    public string productName { get; set; } = string.Empty;
    public string? productImageUrl { get; set; }
    public decimal price { get; set; }
    public int quantity { get; set; }
    public decimal subtotal => price * quantity;
}

public class OrderStaffResponseDto : OrderResponseDto
{
    public string customerName { get; set; } = string.Empty;
    public string customerPhone { get; set; } = string.Empty;
}

public class OrderStaffDetailResponseDto : OrderDetailResponseDto
{
    public string customerEmail { get; set; } = string.Empty;
    public string customerPhone { get; set; } = string.Empty;
    public string customerFullName { get; set; } = string.Empty;
}

public class PaymentResponseDto
{
    public Guid id { get; set; }
    public string paymentMethodName { get; set; } = string.Empty;
    public PaymentStatus status { get; set; }
    public decimal amount { get; set; }
    public string? transactionRef { get; set; }
}

public class OrderRepayResponseDto
{
    public string? checkoutUrl { get; set; }
}
