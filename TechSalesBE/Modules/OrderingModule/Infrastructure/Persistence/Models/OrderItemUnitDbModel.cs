using OrderingModule.Domain.Enums;

namespace OrderingModule.Infrastructure.Persistence.Models;

internal sealed class OrderItemUnitDbModel
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public OrderItemStatus? Status { get; set; }

    public OrderItemDbModel OrderItem { get; set; } = null!;
}
