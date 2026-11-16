using OrderingModule.Domain.Enums;

namespace OrderingModule.Infrastructure.Persistence.Models;

internal sealed class OrderDbModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; }
    public long TotalProductAmount { get; set; }
    public long ShippingFee { get; set; }
    public long DiscountAmount { get; set; }
    public long TotalAmount { get; set; }
    public string ShippingAddressSnapshot { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<OrderItemDbModel> Items { get; set; } = new HashSet<OrderItemDbModel>();
}
