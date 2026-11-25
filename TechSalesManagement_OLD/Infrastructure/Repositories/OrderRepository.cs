using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly TechSalesDbContext _dbContext;

    public OrderRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddOrderAsync(Order order, Guid? voucherId, Guid paymentMethodId)
    {
        var dbOrder = new OrderDbModel
        {
            id = order.id,
            user_id = order.userId,
            status = order.status,
            total_product_amount = order.totalProductAmount,
            shipping_fee = order.shippingFee,
            discount_amount = order.discountAmount,
            total_amount = order.totalAmount,
            shipping_address_snapshot = order.shippingAddressSnapshot,
            created_at = order.createdAt,
            updated_at = order.updatedAt,
            approved_at = order.approvedAt,
            shipped_at = order.shippedAt,
            delivered_at = order.deliveredAt
        };

        var paymentMethod = await _dbContext.PaymentMethods.FindAsync(paymentMethodId);

        if (paymentMethod == null)
        {
            throw new System.Exception($"Payment method with ID '{paymentMethodId}' does not exist in the database settings.");
        }

        await _dbContext.Orders.AddAsync(dbOrder);

        foreach (var item in order.items)
        {
            await _dbContext.OrderItems.AddAsync(new OrderItemDbModel
            {
                order_id = order.id,
                product_id = item.product_id,
                price = item.price,
                quantity = item.quantity
            });
        }

        if (voucherId.HasValue)
        {
            await _dbContext.OrderVouchers.AddAsync(new OrderVoucherDbModel
            {
                order_id = order.id,
                voucher_id = voucherId.Value
            });
        }

        await _dbContext.Payments.AddAsync(new PaymentDbModel
        {
            id = Guid.NewGuid(),
            order_id = order.id,
            payment_method_id = paymentMethod.id,
            status = PaymentStatus.PENDING,
            amount = order.totalAmount,
            created_at = DateTimeOffset.UtcNow,
            updated_at = DateTimeOffset.UtcNow
        });
    }

    public async Task<(List<(Order order, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> orders, int totalCount)> GetOrdersByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _dbContext.Orders
            .Include(o => o.payments)
                .ThenInclude(p => p.payment_method)
            .Where(o => o.user_id == userId);

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(o => o.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var entities = dbModels.Select(m => (MapToEntity(m)!, m.payments.Select(p => (new Payment
        {
            id = p.id,
            orderId = p.order_id,
            paymentMethodId = p.payment_method_id,
            status = p.status,
            amount = p.amount,
            transactionRef = p.transaction_ref,
            createdAt = p.created_at,
            updatedAt = p.updated_at
        }, p.payment_method?.name ?? "Unknown", p.payment_method?.type ?? PaymentMethodType.CASH)).ToList())).ToList();

        return (entities, totalCount);
    }

    public async Task<Order?> GetOrderDetailsByIdAsync(Guid orderId)
    {
        var dbModel = await _dbContext.Orders
            .Include(o => o.order_items)
                .ThenInclude(oi => oi.product)
                    .ThenInclude(p => p.product_images)
            .Include(o => o.order_vouchers)
                .ThenInclude(ov => ov.voucher)
            .FirstOrDefaultAsync(o => o.id == orderId);

        return MapToEntity(dbModel);
    }

    public async Task<(List<(Order order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> orders, int totalCount)> GetOrdersByStatusAsync(OrderStatus status, int pageNumber, int pageSize)
    {
        var query = _dbContext.Orders
            .Include(o => o.user)
                .ThenInclude(u => u.user_profile)
            .Include(o => o.payments)
                .ThenInclude(p => p.payment_method)
            .Where(o => o.status == status);

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(o => o.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var results = dbModels.Select(m => (MapToEntity(m)!, MapUserToEntity(m.user), m.payments.Select(p => (new Payment
        {
            id = p.id,
            orderId = p.order_id,
            paymentMethodId = p.payment_method_id,
            status = p.status,
            amount = p.amount,
            transactionRef = p.transaction_ref,
            createdAt = p.created_at,
            updatedAt = p.updated_at
        }, p.payment_method?.name ?? "Unknown", p.payment_method?.type ?? PaymentMethodType.CASH)).ToList())).ToList();

        return (results, totalCount);
    }

    public async Task<(Order? order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)?> GetOrderWithFullDetailsByIdAsync(Guid orderId)
    {
        var dbModel = await _dbContext.Orders
            .Include(o => o.user)
                .ThenInclude(u => u.user_profile)
            .Include(o => o.order_items)
                .ThenInclude(oi => oi.product)
                    .ThenInclude(p => p.product_images)
            .Include(o => o.payments)
                .ThenInclude(p => p.payment_method)
            .Include(o => o.order_vouchers)
                .ThenInclude(ov => ov.voucher)
            .FirstOrDefaultAsync(o => o.id == orderId);

        if (dbModel == null) return null;

        var order = MapToEntity(dbModel);
        var user = MapUserToEntity(dbModel.user);
        var payments = dbModel.payments.Select(p => (new Payment
        {
            id = p.id,
            orderId = p.order_id,
            paymentMethodId = p.payment_method_id,
            status = p.status,
            amount = p.amount,
            transactionRef = p.transaction_ref,
            createdAt = p.created_at,
            updatedAt = p.updated_at
        }, p.payment_method?.name ?? "Unknown", p.payment_method?.type ?? PaymentMethodType.CASH)).ToList();

        return (order, user, payments);
    }

    private User? MapUserToEntity(UserDbModel? dbModel)
    {
        if (dbModel == null) return null;
        var user = new User
        {
            id = dbModel.id,
            email = dbModel.email,
            status = dbModel.status,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at
        };

        if (dbModel.user_profile != null)
        {
            user.profile = new UserProfile
            {
                userId = dbModel.user_profile.user_id,
                fullName = dbModel.user_profile.full_name,
                phone = dbModel.user_profile.phone,
                avatarUrl = dbModel.user_profile.avatar_url,
                dateOfBirth = dbModel.user_profile.date_of_birth
            };
        }

        return user;
    }

    private Order? MapToEntity(OrderDbModel? dbModel)
    {
        if (dbModel == null) return null;

        var order = new Order
        {
            id = dbModel.id,
            userId = dbModel.user_id,
            status = dbModel.status,
            totalProductAmount = dbModel.total_product_amount,
            shippingFee = dbModel.shipping_fee,
            discountAmount = dbModel.discount_amount,
            totalAmount = dbModel.total_amount,
            shippingAddressSnapshot = dbModel.shipping_address_snapshot,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at,
            approvedAt = dbModel.approved_at,
            shippedAt = dbModel.shipped_at,
            deliveredAt = dbModel.delivered_at
        };

        if (dbModel.order_items != null && dbModel.order_items.Any())
        {
            order.items = dbModel.order_items.Select(oi => new OrderItem
            {
                order_id = oi.order_id,
                product_id = oi.product_id,
                price = oi.price,
                quantity = oi.quantity,
                product = oi.product != null ? new Product
                {
                    id = oi.product.id,
                    name = oi.product.name,
                    images = oi.product.product_images != null ? oi.product.product_images.Select(img => new ProductImage
                    {
                        id = img.id,
                        imageUrl = img.image_url,
                        isPrimary = img.is_primary
                    }).ToList() : new List<ProductImage>()
                } : null
            }).ToList();
        }

        if (dbModel.order_vouchers != null && dbModel.order_vouchers.Any())
        {
            order.vouchers = dbModel.order_vouchers.Select(ov => new Voucher
            {
                id = ov.voucher_id,
                code = ov.voucher?.code ?? string.Empty
            }).ToList();
        }

        return order;
    }

    public async Task CancelOrderAsync(Guid orderId)
    {
        var dbOrder = await _dbContext.Orders.FindAsync(orderId);
        if (dbOrder != null)
        {
            dbOrder.status = OrderStatus.CANCELLED;
            dbOrder.updated_at = DateTimeOffset.UtcNow;
            _dbContext.Orders.Update(dbOrder);

            var payments = await _dbContext.Payments
                .Where(p => p.order_id == orderId)
                .ToListAsync();

            foreach (var payment in payments)
            {
                payment.status = PaymentStatus.CANCELLED;
                payment.updated_at = DateTimeOffset.UtcNow;
                _dbContext.Payments.Update(payment);
            }
        }
    }

    public async Task UpdateStatusAsync(Guid orderId, OrderStatus status)
    {
        var dbOrder = await _dbContext.Orders.FindAsync(orderId);
        if (dbOrder != null)
        {
            dbOrder.status = status;
            dbOrder.updated_at = DateTimeOffset.UtcNow;
            
            switch (status)
            {
                case OrderStatus.APPROVED:
                    dbOrder.approved_at = DateTimeOffset.UtcNow;
                    break;
                case OrderStatus.SHIPPING:
                    dbOrder.shipped_at = DateTimeOffset.UtcNow;
                    break;
                case OrderStatus.DELIVERED:
                    dbOrder.delivered_at = DateTimeOffset.UtcNow;
                    var pendingPayments = await _dbContext.Payments
                        .Include(p => p.payment_method)
                        .Where(p => p.order_id == orderId && p.status == PaymentStatus.PENDING)
                        .ToListAsync();
                    foreach (var payment in pendingPayments)
                    {
                        if (payment.payment_method?.type == PaymentMethodType.CASH)
                        {
                            payment.status = PaymentStatus.SUCCESS;
                            payment.updated_at = DateTimeOffset.UtcNow;
                        }
                    }
                    break;
            }

            _dbContext.Orders.Update(dbOrder);
        }
    }

    public async Task<(List<(Order order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> orders, int totalCount)> GetRefundableOrdersAsync(int pageNumber, int pageSize)
    {
        var query = _dbContext.Orders
            .Include(o => o.user)
                .ThenInclude(u => u.user_profile)
            .Include(o => o.payments)
                .ThenInclude(p => p.payment_method)
            .Where(o => o.status == OrderStatus.CANCELLED);

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(o => o.created_at)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var results = dbModels.Select(m => (
            MapToEntity(m)!,
            MapUserToEntity(m.user),
            m.payments.Select(p => (new Payment
            {
                id = p.id,
                orderId = p.order_id,
                paymentMethodId = p.payment_method_id,
                status = p.status,
                amount = p.amount,
                transactionRef = p.transaction_ref,
                createdAt = p.created_at,
                updatedAt = p.updated_at
            }, p.payment_method?.name ?? "Unknown", p.payment_method?.type ?? PaymentMethodType.CASH)).ToList()
        )).ToList();

        return (results, totalCount);
    }

    public async Task<(List<(Order order, User? user, List<(Payment payment, string methodName, PaymentMethodType type)> payments)> orders, int totalCount)> SearchOrdersAsync(TechSalesManagement.Domain.Specifications.OrderSearchParameters parameters)
    {
        var query = _dbContext.Orders
            .Include(o => o.user)
                .ThenInclude(u => u.user_profile)
            .Include(o => o.payments)
                .ThenInclude(p => p.payment_method)
            .AsQueryable();

        query = TechSalesManagement.Domain.Specifications.OrderSearchSpecification.ApplyFilters(query, parameters);

        int totalCount = await query.CountAsync();

        var dbModels = await query
            .OrderByDescending(o => o.created_at)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var results = dbModels.Select(m => (
            MapToEntity(m)!,
            MapUserToEntity(m.user),
            m.payments.Select(p => (new Payment
            {
                id = p.id,
                orderId = p.order_id,
                paymentMethodId = p.payment_method_id,
                status = p.status,
                amount = p.amount,
                transactionRef = p.transaction_ref,
                createdAt = p.created_at,
                updatedAt = p.updated_at
            }, p.payment_method?.name ?? "Unknown", p.payment_method?.type ?? PaymentMethodType.CASH)).ToList()
        )).ToList();

        return (results, totalCount);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        var dbModel = await _dbContext.Orders.FindAsync(order.id);
        if (dbModel != null)
        {
            dbModel.status = order.status;
            dbModel.updated_at = order.updatedAt;
            dbModel.approved_at = order.approvedAt;
            dbModel.shipped_at = order.shippedAt;
            dbModel.delivered_at = order.deliveredAt;
            _dbContext.Orders.Update(dbModel);
        }
    }
}
