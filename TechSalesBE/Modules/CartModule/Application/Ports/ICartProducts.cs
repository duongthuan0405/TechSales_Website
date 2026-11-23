namespace CartModule.Application.Ports;

public interface ICartProducts
{
    Task<List<CartProduct>> GetProductsAsync(List<Guid> productIds, CancellationToken cancellationToken);
}

public sealed record CartProduct(Guid Id, string Name, string Description, long? Price, string Status,
    Guid BrandId, string BrandName, Guid CategoryId, string CategoryName, List<CartProductImage> Images);
public sealed record CartProductImage(Guid Id, string ImageUrl, bool IsPrimary);
