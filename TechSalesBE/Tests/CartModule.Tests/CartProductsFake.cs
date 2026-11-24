using CartModule.Application.Ports;

namespace CartModule.Tests;

internal sealed class CartProductsFake : ICartProducts
{
    public string Status { get; set; } = "ACTIVE";
    public HashSet<Guid> MissingIds { get; } = new HashSet<Guid>();
    public List<List<Guid>> Calls { get; } = new List<List<Guid>>();

    public Task<List<CartProduct>> GetProductsAsync(List<Guid> productIds, CancellationToken cancellationToken)
    {
        Calls.Add(productIds.ToList());
        return Task.FromResult(productIds.Where(id => !MissingIds.Contains(id)).Select(id => new CartProduct(
            id, "Laptop", "Details", 15000000, Status, Guid.NewGuid(), "Brand", Guid.NewGuid(), "Category",
            new List<CartProductImage> { new CartProductImage(Guid.NewGuid(), "/product-images/test.png", true) })).ToList());
    }
}
