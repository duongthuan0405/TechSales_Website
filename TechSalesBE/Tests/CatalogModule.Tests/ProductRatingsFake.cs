using CatalogModule.Application.Ports;

namespace CatalogModule.Tests;

internal sealed class ProductRatingsFake : IProductRatings
{
    public List<ProductRating> Ratings { get; } = new List<ProductRating>();

    public Task<List<ProductRating>> GetRatingsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Ratings.ToList());
    }
}
