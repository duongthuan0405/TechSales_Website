using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Domain.Specifications;

namespace TechSalesManagement.Application.Services.Interfaces;

public interface IOrderManagementService
{
    Task<(List<(Order order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> items, int totalCount)> SearchOrdersAsync(OrderSearchParameters parameters);
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus nextStatus, Guid staffId);
    Task<(Order? order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> GetOrderDetailsAsync(Guid orderId);
}
