namespace CartModule.Infrastructure.Persistence.Models;

internal sealed class CartItemDbModel
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public CartDbModel Cart { get; set; } = null!;
}
