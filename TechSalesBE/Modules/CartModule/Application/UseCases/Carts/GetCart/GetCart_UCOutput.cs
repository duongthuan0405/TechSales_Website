namespace CartModule.Application.UseCases.Carts.GetCart;

internal sealed record GetCart_UCOutput(Guid CartId, List<GetCart_UCOutput.Item> Items)
{
    internal sealed record Item(Guid ProductId, int Quantity, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
}
