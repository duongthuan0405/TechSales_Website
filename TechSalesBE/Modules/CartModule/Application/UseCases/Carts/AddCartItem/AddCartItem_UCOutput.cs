namespace CartModule.Application.UseCases.Carts.AddCartItem;

internal sealed record AddCartItem_UCOutput(Guid CartId, Guid ProductId, int Quantity);
