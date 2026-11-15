using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Domain.Specifications;
using TechSalesManagement.Common;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Staff,Business Admin,Technical Admin")]
public class OrderManagementController : ControllerBase
{
    private readonly IOrderManagementService _orderManagementService;

    public OrderManagementController(IOrderManagementService orderManagementService)
    {
        _orderManagementService = orderManagementService;
    }

    private (string paymentMethodName, bool? isPaymentFailed) GetPaymentInfo(System.Collections.Generic.List<(TechSalesManagement.Domain.Entities.Payment payment, string methodName, TechSalesManagement.Domain.Enums.PaymentMethodType type)> payments)
    {
        if (payments == null || !payments.Any())
            return (string.Empty, null);

        var successPaymentTuple = payments.FirstOrDefault(p => p.payment.status == TechSalesManagement.Domain.Enums.PaymentStatus.SUCCESS);
        var latestPaymentTuple = payments.OrderByDescending(p => p.payment.createdAt).FirstOrDefault();

        var paymentTuple = successPaymentTuple.payment != null ? successPaymentTuple : latestPaymentTuple;

        bool? isFailed = null;
        if (paymentTuple.type == TechSalesManagement.Domain.Enums.PaymentMethodType.ONLINE)
        {
            isFailed = successPaymentTuple.payment == null && latestPaymentTuple.payment?.status == TechSalesManagement.Domain.Enums.PaymentStatus.FAILED;
        }

        return (paymentTuple.methodName ?? string.Empty, isFailed);
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<OrderAdminSummaryDto>>>> GetOrdersAsync([FromQuery] OrderSearchParameters parameters)
    {
        var (items, totalCount) = await _orderManagementService.SearchOrdersAsync(parameters);
        
        var results = items.Select(i => {
            var paymentInfo = GetPaymentInfo(i.payments);
            return new OrderAdminSummaryDto
            {
                orderId = i.order.id,
                customerEmail = i.user?.email ?? "Unknown",
                customerName = i.user?.profile?.fullName ?? "Unknown",
                status = i.order.status,
                totalAmount = i.order.totalAmount,
                createdAt = i.order.createdAt,
                paymentMethodName = paymentInfo.paymentMethodName,
                isPaymentFailed = paymentInfo.isPaymentFailed
            };
        }).ToList();

        var response = new PagedResponseDto<OrderAdminSummaryDto>
        {
            items = results,
            totalCount = totalCount,
            pageNumber = parameters.PageNumber,
            pageSize = parameters.PageSize
        };

        return Ok(new ApiSuccessResponse<PagedResponseDto<OrderAdminSummaryDto>>(response, "Orders retrieved successfully."));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> GetDetailsAsync([FromRoute] Guid id)
    {
        var (order, user, payments) = await _orderManagementService.GetOrderDetailsAsync(id);
        
        var result = new
        {
            order = order,
            customer = user,
            payments = payments.Select(p => new { p.payment, p.methodName, p.type })
        };

        return Ok(new ApiSuccessResponse<object>(result, "Order details retrieved successfully."));
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UpdateStatusAsync([FromRoute] Guid id, [FromBody] UpdateOrderStatusRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _orderManagementService.UpdateOrderStatusAsync(id, request.status, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, "Order status updated successfully."));
    }
}

public class OrderAdminSummaryDto
{
    public Guid orderId { get; set; }
    public string customerEmail { get; set; } = string.Empty;
    public string customerName { get; set; } = string.Empty;
    public OrderStatus status { get; set; }
    public decimal totalAmount { get; set; }
    public DateTimeOffset createdAt { get; set; }
    public string paymentMethodName { get; set; } = string.Empty;
    public bool? isPaymentFailed { get; set; }
}

public class UpdateOrderStatusRequestDto
{
    public OrderStatus status { get; set; }
}
