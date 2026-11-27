namespace CatalogModule.Application.Ports;

public interface IProductRatings
{
    Task<List<ProductRating>> GetRatingsAsync(CancellationToken cancellationToken);
}

public sealed record ProductRating(Guid ProductId, double AverageRating, int ReviewCount);
