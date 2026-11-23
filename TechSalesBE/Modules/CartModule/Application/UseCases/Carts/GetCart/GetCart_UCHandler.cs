using CartModule.Application.Repositories;
using CartModule.Domain.Entities;
using MediatR;
using CartModule.Application.Ports;

namespace CartModule.Application.UseCases.Carts.GetCart;

internal sealed class GetCart_UCHandler : IRequestHandler<GetCart_UCInput, GetCart_UCOutput>
{
    private readonly ICartRepository _carts;
    private readonly ICartProducts _products;

    public GetCart_UCHandler(ICartRepository carts, ICartProducts products)
    {
        _carts = carts;
        _products = products;
    }

    public async Task<GetCart_UCOutput> Handle(GetCart_UCInput input, CancellationToken cancellationToken)
    {
        Cart? cart = await _carts.GetByUserIdAsync(input.UserId, cancellationToken);
        if (cart == null)
        {
            Cart newCart = new Cart(Guid.NewGuid(), input.UserId);
            bool created = await _carts.TryCreateAsync(newCart, cancellationToken);
            if (created)
            {
                cart = newCart;
            }
            else
            {
                cart = await _carts.GetByUserIdAsync(input.UserId, cancellationToken)
                    ?? throw new InvalidOperationException("The existing cart could not be loaded after a concurrent insert.");
            }
        }

        List<CartProduct> products = await _products.GetProductsAsync(
            cart.Items.Select(item => item.ProductId).Distinct().ToList(), cancellationToken);
        Dictionary<Guid, CartProduct> productMap = products.ToDictionary(product => product.Id);
        List<GetCart_UCOutput.Item> items = cart.Items
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.ProductId)
            .Select(item => new GetCart_UCOutput.Item(item.ProductId, item.Quantity, item.CreatedAt, item.UpdatedAt,
                productMap.GetValueOrDefault(item.ProductId)))
            .ToList();

        return new GetCart_UCOutput(cart.Id, items);
    }
}
