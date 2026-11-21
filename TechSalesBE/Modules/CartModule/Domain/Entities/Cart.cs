using CartModule.Domain.Exceptions;
using CartModule.Domain.Rules;
using CommonModule.Domain.Exceptions;

namespace CartModule.Domain.Entities;

internal sealed class Cart
{
    private readonly Dictionary<Guid, CartItem> _items = new();

    public Guid Id { get; }
    public Guid UserId { get; }
    public IReadOnlyCollection<CartItem> Items => _items.Values;

    public Cart(Guid id, Guid userId)
        : this(id, userId, [])
    {
    }

    public Cart(Guid id, Guid userId, IEnumerable<CartItem> items)
    {
        CartRules.EnsureValidId(id, nameof(Id));
        CartRules.EnsureValidId(userId, nameof(UserId));

        Id = id;
        UserId = userId;

        foreach (var item in items)
        {
            CartRules.EnsureItemBelongsToCart(Id, item.CartId);
            if (!_items.TryAdd(item.ProductId, item))
            {
                throw new DomainException(DomainExceptionCode.DuplicateProduct, DomainExceptionMessage.DuplicateProduct);
            }
        }
    }

    public CartItem AddItem(Guid productId, DateTimeOffset addedAt, int quantity = 1)
    {
        CartRules.EnsureValidId(productId, nameof(CartItem.ProductId));
        if (_items.TryGetValue(productId, out var item))
        {
            var totalQuantity = CartRules.GetIncreasedQuantity(item.Quantity, quantity);
            item.UpdateQuantity(totalQuantity, addedAt);
            return item;
        }

        CartItem newItem = new CartItem(Id, productId, quantity, addedAt, addedAt);
        _items.Add(productId, newItem);
        return newItem;
    }

    public void UpdateItemQuantity(Guid productId, int quantity, DateTimeOffset updatedAt)
    {
        CartRules.EnsureValidId(productId, nameof(CartItem.ProductId));
        if (!_items.TryGetValue(productId, out var item))
        {
            throw new DomainException(DomainExceptionCode.ItemNotFound, DomainExceptionMessage.ItemNotFound);
        }

        item.UpdateQuantity(quantity, updatedAt);
    }

    public bool RemoveItem(Guid productId)
    {
        CartRules.EnsureValidId(productId, nameof(CartItem.ProductId));
        return _items.Remove(productId);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
