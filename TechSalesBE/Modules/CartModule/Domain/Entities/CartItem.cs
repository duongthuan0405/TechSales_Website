using CartModule.Domain.Rules;

namespace CartModule.Domain.Entities;

internal sealed class CartItem
{
    public Guid CartId { get; }
    public Guid ProductId { get; }
    public int Quantity { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public CartItem(Guid cartId, Guid productId, int quantity, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        CartRules.EnsureValidId(cartId, nameof(CartId));
        CartRules.EnsureValidId(productId, nameof(ProductId));
        CartRules.EnsurePositiveQuantity(quantity);
        CartRules.EnsureTimestampOrder(createdAt, updatedAt);

        CartId = cartId;
        ProductId = productId;
        Quantity = quantity;
        CreatedAt = createdAt.ToUniversalTime();
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    internal void UpdateQuantity(int quantity, DateTimeOffset updatedAt)
    {
        CartRules.EnsurePositiveQuantity(quantity);
        if (quantity == Quantity)
        {
            return;
        }

        CartRules.EnsureTimestampOrder(UpdatedAt, updatedAt);
        Quantity = quantity;
        UpdatedAt = updatedAt.ToUniversalTime();
    }
}
