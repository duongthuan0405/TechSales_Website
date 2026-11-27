using CatalogModule.Application.Ports;

namespace TechSalesBE.WebAPI.Integration;

internal sealed class ProductRatingsAdapter : IProductRatings
{
    private readonly ReviewNotificationModule.Contracts.IProductRatings _ratings;

    public ProductRatingsAdapter(ReviewNotificationModule.Contracts.IProductRatings ratings)
    {
        _ratings = ratings;
    }

    public async Task<List<ProductRating>> GetRatingsAsync(CancellationToken cancellationToken)
    {
        var ratings = await _ratings.GetRatingsAsync(cancellationToken);
        return ratings.Select(rating => new ProductRating(rating.ProductId, rating.AverageRating, rating.ReviewCount)).ToList();
    }
}
