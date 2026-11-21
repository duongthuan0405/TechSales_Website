namespace CartModule.Application.UseCases.Carts.UpdateCartItem;

internal sealed record UpdateCartItem_UCOutput(Guid CartId, Guid ProductId, int Quantity);
