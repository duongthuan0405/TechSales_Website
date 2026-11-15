using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/order")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
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

    [HttpPost]
    public async Task<ActionResult<ApiSuccessResponse<OrderResponseDto>>> PlaceOrderAsync([FromBody] PlaceOrderRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new PlaceOrderParams
        {
            UserId = userId.Value,
            ProductsWithQuantity = request.productsWithQuantity,
            ShippingAddressId = request.shippingAddressId,
            PaymentMethodId = request.paymentMethodId,
            VoucherCode = request.voucherCode
        };

        var (newOrder, checkoutUrl) = await _orderService.PlaceOrderAsync(parameters);

        var response = new OrderResponseDto
        {
            id = newOrder.id,
            status = newOrder.status,
            totalAmount = newOrder.totalAmount,
            createdAt = newOrder.createdAt,
            checkoutUrl = checkoutUrl
        };

        // BR91: Returns 200-OK with MSG37
        return Ok(new ApiSuccessResponse<OrderResponseDto>(response, MessageConstants.MSG37));
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<OrderResponseDto>>>> GetOrderHistoryAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new GetOrderHistoryParams
        {
            UserId = userId.Value,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (orders, totalCount) = await _orderService.GetOrderHistoryAsync(parameters);

        var items = orders.Select(o => {
            var paymentInfo = GetPaymentInfo(o.payments);
            return new OrderResponseDto
            {
                id = o.order.id,
                status = o.order.status,
                totalAmount = o.order.totalAmount,
                createdAt = o.order.createdAt,
                paymentMethodName = paymentInfo.paymentMethodName,
                isPaymentFailed = paymentInfo.isPaymentFailed
            };
        }).ToList();

        var response = new  PagedResponseDto<OrderResponseDto>
        {
            items = items,
            pageNumber = pageNumber,
            pageSize = pageSize,
            totalCount = totalCount
        };

        // BR108: Empty state returns MSG42
        string message = totalCount == 0 ? MessageConstants.MSG42 : "Order history retrieved successfully.";
        return Ok(new ApiSuccessResponse<PagedResponseDto<OrderResponseDto>>(response, message));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiSuccessResponse<OrderDetailResponseDto>>> GetOrderDetailsAsync([FromRoute] Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new GetOrderDetailsParams
        {
            OrderId = id,
            UserId = userId.Value
        };

        var result = await _orderService.GetOrderWithFullDetailsAsync(id);
        
        // Verify user owns the order
        if (result.order.userId != userId.Value)
        {
            return Forbid();
        }

        var order = result.order;

        var paymentInfo = GetPaymentInfo(result.payments);

        var response = new OrderStaffDetailResponseDto
        {
            id = order.id,
            status = order.status,
            totalProductAmount = order.totalProductAmount,
            shippingFee = order.shippingFee,
            discountAmount = order.discountAmount,
            totalAmount = order.totalAmount,
            shippingAddressSnapshot = order.shippingAddressSnapshot,
            createdAt = order.createdAt,
            approvedAt = order.approvedAt,
            shippedAt = order.shippedAt,
            deliveredAt = order.deliveredAt,
            paymentMethodName = paymentInfo.paymentMethodName,
            isPaymentFailed = paymentInfo.isPaymentFailed,
            items = order.items.Select(i => new OrderItemResponseDto
            {
                productId = i.product_id,
                productName = i.product?.name ?? "Unknown Product",
                productImageUrl = i.product?.images?.FirstOrDefault(img => img.isPrimary)?.imageUrl ?? i.product?.images?.FirstOrDefault()?.imageUrl,
                price = i.price,
                quantity = i.quantity
            }).ToList(),
            payments = result.payments.Select(x => new PaymentResponseDto
            {
                id = x.payment.id,
                paymentMethodName = x.methodName,
                status = x.payment.status,
                amount = x.payment.amount,
                transactionRef = x.payment.transactionRef
            }).ToList()
        };

        return Ok(new ApiSuccessResponse<OrderStaffDetailResponseDto>(response, "Order details retrieved successfully."));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> CancelOrderAsync([FromRoute] Guid id)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new CancelOrderParams
        {
            OrderId = id,
            UserId = userId.Value
        };

        await _orderService.CancelOrderAsync(parameters);

        // BR122: Returns 200-OK with MSG46
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG46));
    }

    [HttpPost("{id}/repay")]
    public async Task<ActionResult<ApiSuccessResponse<OrderRepayResponseDto>>> CreateRepaySessionAsync([FromRoute] Guid id, [FromBody] OrderRepayRequestDto? request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var checkoutUrl = await _orderService.CreateRepaySessionAsync(id, userId.Value, request?.paymentMethodId);

        var response = new OrderRepayResponseDto
        {
            checkoutUrl = checkoutUrl
        };

        return Ok(new ApiSuccessResponse<OrderRepayResponseDto>(response, "Repayment session created successfully."));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpGet("pending")]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<OrderStaffResponseDto>>>> GetPendingOrdersAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var parameters = new GetPendingOrdersParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (orders, totalCount) = await _orderService.GetPendingOrdersAsync(parameters);

        var items = orders.Select(x => {
            var paymentInfo = GetPaymentInfo(x.payments);
            return new OrderStaffResponseDto
            {
                id = x.order.id,
                status = x.order.status,
                totalAmount = x.order.totalAmount,
                createdAt = x.order.createdAt,
                customerName = x.user?.profile?.fullName ?? "Unknown",
                customerPhone = x.user?.profile?.phone ?? "N/A",
                paymentMethodName = paymentInfo.paymentMethodName,
                isPaymentFailed = paymentInfo.isPaymentFailed
            };
        }).ToList();

        var response = new PagedResponseDto<OrderStaffResponseDto>
        {
            items = items,
            pageNumber = pageNumber,
            pageSize = pageSize,
            totalCount = totalCount
        };

        // BR143: Empty state returns MSG52
        string message = totalCount == 0 ? MessageConstants.MSG52 : MessageConstants.MSG119;
        return Ok(new ApiSuccessResponse<PagedResponseDto<OrderStaffResponseDto>>(response, message));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpGet("{id}/staff")]
    public async Task<ActionResult<ApiSuccessResponse<OrderStaffDetailResponseDto>>> GetOrderWithFullDetailsAsync([FromRoute] Guid id)
    {
        var (order, user, payments) = await _orderService.GetOrderWithFullDetailsAsync(id);

        var paymentInfo = GetPaymentInfo(payments);

        var response = new OrderStaffDetailResponseDto
        {
            id = order.id,
            status = order.status,
            totalProductAmount = order.totalProductAmount,
            shippingFee = order.shippingFee,
            discountAmount = order.discountAmount,
            totalAmount = order.totalAmount,
            shippingAddressSnapshot = order.shippingAddressSnapshot,
            createdAt = order.createdAt,
            approvedAt = order.approvedAt,
            shippedAt = order.shippedAt,
            deliveredAt = order.deliveredAt,
            customerEmail = user?.email ?? string.Empty,
            customerFullName = user?.profile?.fullName ?? string.Empty,
            customerPhone = user?.profile?.phone ?? string.Empty,
            paymentMethodName = paymentInfo.paymentMethodName,
            isPaymentFailed = paymentInfo.isPaymentFailed,
            items = order.items.Select(i => new OrderItemResponseDto
            {
                productId = i.product_id,
                productName = i.product?.name ?? "Unknown Product",
                productImageUrl = i.product?.images?.FirstOrDefault(img => img.isPrimary)?.imageUrl ?? i.product?.images?.FirstOrDefault()?.imageUrl,
                price = i.price,
                quantity = i.quantity
            }).ToList(),
            payments = payments.Select(x => new PaymentResponseDto
            {
                id = x.payment.id,
                paymentMethodName = x.methodName,
                status = x.payment.status,
                amount = x.payment.amount,
                transactionRef = x.payment.transactionRef
            }).ToList()
        };

        return Ok(new ApiSuccessResponse<OrderStaffDetailResponseDto>(response, MessageConstants.MSG120));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> ApproveOrderAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        var parameters = new ApproveOrderParams
        {
            OrderId = id,
            StaffId = staffId.Value
        };

        await _orderService.ApproveOrderAsync(parameters);

        // BR145: Returns 200-OK with MSG55
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG55));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpPost("{id}/ship")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> ShipOrderAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _orderService.ShipOrderAsync(id, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG121));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpPost("{id}/confirm-delivery")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> ConfirmDeliveryAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _orderService.ConfirmDeliveryAsync(id, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG122));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpPost("{id}/staff-cancel")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> StaffCancelOrderAsync([FromRoute] Guid id, [FromBody] OrderStaffCancelRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _orderService.StaffCancelOrderAsync(id, staffId.Value, request.reason);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG58));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpPost("{id}/refund")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> InitiateRefundAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _orderService.InitiateRefundAsync(id, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG62));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpGet("refund-requests")]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<OrderStaffResponseDto>>>> GetRefundRequestsAsync([FromQuery] GetPendingOrdersParams parameters)
    {
        var (orders, totalCount) = await _orderService.GetRefundRequestsAsync(parameters.PageNumber, parameters.PageSize);

        var response = new PagedResponseDto<OrderStaffResponseDto>
        {
            items = orders.Select(o => {
                var paymentInfo = GetPaymentInfo(o.payments);
                return new OrderStaffResponseDto
                {
                    id = o.order.id,
                    customerName = o.user?.profile?.fullName ?? "Unknown",
                    totalAmount = o.order.totalAmount,
                    status = o.order.status,
                    createdAt = o.order.createdAt,
                    paymentMethodName = paymentInfo.paymentMethodName,
                    isPaymentFailed = paymentInfo.isPaymentFailed
                };
            }).ToList(),
            totalCount = totalCount,
            pageNumber = parameters.PageNumber,
            pageSize = parameters.PageSize
        };

        return Ok(new ApiSuccessResponse<PagedResponseDto<OrderStaffResponseDto>>(response, "Refund requests retrieved successfully."));
    }

    [Authorize(Roles = "Staff,Business Admin,Technical Admin")]
    [HttpGet("search")]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<OrderStaffResponseDto>>>> SearchOrdersAsync([FromQuery] OrderSearchRequestDto request)
    {
        var parameters = new TechSalesManagement.Domain.Specifications.OrderSearchParameters
        {
            OrderCode = request.orderCode,
            CustomerName = request.customerName,
            PhoneNumber = request.phoneNumber,
            FromDate = request.fromDate,
            ToDate = request.toDate,
            PageNumber = request.pageNumber,
            PageSize = request.pageSize
        };

        var (orders, totalCount) = await _orderService.SearchOrdersAsync(parameters);

        var response = new PagedResponseDto<OrderStaffResponseDto>
        {
            items = orders.Select(o => {
                var paymentInfo = GetPaymentInfo(o.payments);
                return new OrderStaffResponseDto
                {
                    id = o.order.id,
                    customerName = o.user?.profile?.fullName ?? "Unknown",
                    totalAmount = o.order.totalAmount,
                    status = o.order.status,
                    createdAt = o.order.createdAt,
                    paymentMethodName = paymentInfo.paymentMethodName,
                    isPaymentFailed = paymentInfo.isPaymentFailed
                };
            }).ToList(),
            totalCount = totalCount,
            pageNumber = request.pageNumber,
            pageSize = request.pageSize
        };

        return Ok(new ApiSuccessResponse<PagedResponseDto<OrderStaffResponseDto>>(response, "Orders searched successfully."));
    }
}
