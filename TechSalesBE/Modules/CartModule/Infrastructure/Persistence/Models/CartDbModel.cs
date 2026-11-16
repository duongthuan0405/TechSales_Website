namespace CartModule.Infrastructure.Persistence.Models;

internal sealed class CartDbModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public ICollection<CartItemDbModel> Items { get; set; } = new HashSet<CartItemDbModel>();
}
