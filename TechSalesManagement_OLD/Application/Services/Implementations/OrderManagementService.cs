using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.HelperServices;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Domain.Specifications;

namespace TechSalesManagement.Application.Services.Implementations;

public class OrderManagementService : IOrderManagementService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderManagementService(
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IUserRepository userRepository,
        IAuditLogRepository auditLogRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _userRepository = userRepository;
        _auditLogRepository = auditLogRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<(List<(Order order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> items, int totalCount)> SearchOrdersAsync(OrderSearchParameters parameters)
    {
        return await _orderRepository.SearchOrdersAsync(parameters);
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus nextStatus, Guid staffId)
    {
        var result = await _orderRepository.GetOrderWithFullDetailsByIdAsync(orderId);
        if (result == null) throw new NotFoundException("Order not found.");

        var order = result.Value.order;
        if (order == null) throw new NotFoundException("Order not found.");

        var oldStatus = order.status;
        if (oldStatus == nextStatus) return;

        // Strict state transition validation
        if (nextStatus == OrderStatus.APPROVED && oldStatus != OrderStatus.PENDING)
        {
            throw new BadRequestException($"Cannot approve an order that is currently in {oldStatus} status.");
        }
        if (nextStatus == OrderStatus.SHIPPING && oldStatus != OrderStatus.APPROVED)
        {
            throw new BadRequestException($"Cannot ship an order that is currently in {oldStatus} status. Order must be APPROVED first.");
        }
        if (nextStatus == OrderStatus.DELIVERED && oldStatus != OrderStatus.SHIPPING)
        {
            throw new BadRequestException($"Cannot deliver an order that is currently in {oldStatus} status. Order must be SHIPPING first.");
        }
        if (nextStatus == OrderStatus.CANCELLED && (oldStatus == OrderStatus.DELIVERED || oldStatus == OrderStatus.CANCELLED))
        {
            throw new BadRequestException($"Cannot cancel an order that is already in {oldStatus} status.");
        }

        try
        {
            await _unitOfWork.BeginAsync();

            // Transition logic
            switch (nextStatus)
            {
                case OrderStatus.APPROVED:
                    // Check payment status for online orders
                    var payments = result.Value.payments;
                    foreach (var p in payments)
                    {
                        // If it's an online payment (not COD), it must be SUCCESS before approval
                        if (p.methodName.ToUpper() != "COD" && p.payment.status != PaymentStatus.SUCCESS)
                        {
                            throw new BadRequestException("Cannot approve online orders that are not yet paid.");
                        }
                    }
                    order.Approve();
                    break;
                case OrderStatus.SHIPPING:
                    order.Ship();
                    foreach (var item in order.items)
                    {
                        await _inventoryRepository.DeductStockAsync(item.product_id, item.quantity);
                    }
                    break;
                case OrderStatus.DELIVERED:
                    order.Deliver();
                    break;
                case OrderStatus.CANCELLED:
                    var wasShipped = (oldStatus == OrderStatus.SHIPPING);
                    order.Cancel();
                    // Restock logic
                    foreach (var item in order.items)
                    {
                        if (wasShipped) 
                        {
                            await _inventoryRepository.RestorePhysicalStockAsync(item.product_id, item.quantity);
                        } 
                        else 
                        {
                            await _inventoryRepository.ReleaseStockAsync(item.product_id, item.quantity);
                        }
                    }
                    break;
                default:
                    throw new BadRequestException("Invalid status transition.");
            }

            await _orderRepository.UpdateOrderAsync(order);

            var auditLog = new AuditLog(staffId, "UPDATE_ORDER_STATUS", "Orders", orderId.ToString())
            {
                oldValues = System.Text.Json.JsonSerializer.Serialize(new { status = oldStatus.ToString() }),
                newValues = System.Text.Json.JsonSerializer.Serialize(new { status = nextStatus.ToString() }),
                affectedColumns = "status"
            };
            await _auditLogRepository.AddAsync(auditLog);

            await _unitOfWork.FinishAsync();

            // Notify Customer (Best effort, outside transaction)
            try
            {
                var user = await _userRepository.GetByIdAsync(order.userId);
                if (user != null && !string.IsNullOrEmpty(user.email))
                {
                    await _emailService.SendOrderConfirmationEmailAsync(user.email, order.id, order.totalAmount, "Status updated to: " + nextStatus.ToString());
                }
            }
            catch { /* Log error but don't fail */ }
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<(Order? order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> GetOrderDetailsAsync(Guid orderId)
    {
        var result = await _orderRepository.GetOrderWithFullDetailsByIdAsync(orderId);
        if (result == null) throw new NotFoundException("Order not found.");

        return (result.Value.order, result.Value.user, result.Value.payments);
    }
}
