using System;
using System.Linq.Expressions;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Domain.Specifications;

public class RefundableOrderSpecification
{
    public Expression<Func<Order, bool>> ToExpression()
    {
        return order => order.status == OrderStatus.CANCELLED;
        // In a real scenario, we would also check if any payment is COMPLETED but not REFUNDED
    }
}
