using Microsoft.EntityFrameworkCore;
using ReviewNotificationModule.Contracts;
using ReviewNotificationModule.Domain.Enums;
using ReviewNotificationModule.Infrastructure.Persistence;

namespace ReviewNotificationModule.Infrastructure;

internal sealed class ProductRatings : IProductRatings
{
    private readonly ReviewNotificationDbContext _db;

    public ProductRatings(ReviewNotificationDbContext db)
    {
        _db = db;
    }

    public Task<List<ProductRatingSnapshot>> GetRatingsAsync(CancellationToken cancellationToken)
    {
        return _db.Reviews.AsNoTracking().Where(review => review.Status == ReviewStatus.VISIBLE)
            .GroupBy(review => review.ProductId)
            .Select(group => new ProductRatingSnapshot(group.Key, group.Average(review => (double)review.Rating), group.Count()))
            .ToListAsync(cancellationToken);
    }
}
