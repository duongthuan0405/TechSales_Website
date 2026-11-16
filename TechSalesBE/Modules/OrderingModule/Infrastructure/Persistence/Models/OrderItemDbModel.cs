namespace OrderingModule.Infrastructure.Persistence.Models;

internal sealed class OrderItemDbModel
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public long Price { get; set; }
    public int Quantity { get; set; }

    public OrderDbModel Order { get; set; } = null!;
    public ICollection<OrderItemUnitDbModel> Units { get; set; } = new HashSet<OrderItemUnitDbModel>();
}
