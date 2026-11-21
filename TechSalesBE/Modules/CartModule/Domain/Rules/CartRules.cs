using CartModule.Domain.Exceptions;
using CommonModule.Domain.Exceptions;

namespace CartModule.Domain.Rules;

internal static class CartRules
{
    public static bool IsValidId(Guid id)
    {
        return id != Guid.Empty;
    }

    public static void EnsureValidId(Guid id, string name)
    {
        if (!IsValidId(id))
        {
            throw new DomainException(
                DomainExceptionCode.InvalidId,
                string.Format(DomainExceptionMessage.InvalidId, name));
        }
    }

    public static void EnsurePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException(DomainExceptionCode.InvalidQuantity, DomainExceptionMessage.InvalidQuantity);
        }
    }

    public static int GetIncreasedQuantity(int currentQuantity, int addedQuantity)
    {
        EnsurePositiveQuantity(addedQuantity);
        if (addedQuantity > int.MaxValue - currentQuantity)
        {
            throw new DomainException(DomainExceptionCode.QuantityOverflow, DomainExceptionMessage.QuantityOverflow);
        }

        return currentQuantity + addedQuantity;
    }

    public static void EnsureTimestampOrder(DateTimeOffset previousTimestamp, DateTimeOffset updatedAt)
    {
        if (updatedAt < previousTimestamp)
        {
            throw new DomainException(DomainExceptionCode.InvalidTimestamp, DomainExceptionMessage.InvalidTimestamp);
        }
    }

    public static void EnsureItemBelongsToCart(Guid cartId, Guid itemCartId)
    {
        if (itemCartId != cartId)
        {
            throw new DomainException(DomainExceptionCode.ItemCartMismatch, DomainExceptionMessage.ItemCartMismatch);
        }
    }
}
