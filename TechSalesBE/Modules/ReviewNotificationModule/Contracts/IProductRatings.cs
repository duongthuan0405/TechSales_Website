namespace ReviewNotificationModule.Contracts;

public interface IProductRatings
{
    Task<List<ProductRatingSnapshot>> GetRatingsAsync(CancellationToken cancellationToken);
}

public sealed record ProductRatingSnapshot(Guid ProductId, double AverageRating, int ReviewCount);
