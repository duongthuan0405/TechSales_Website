using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReviewNotificationModule.Contracts;
using ReviewNotificationModule.Infrastructure.Persistence;
using Xunit;

namespace ModuleContracts.Tests;

public sealed class ProductRatingsTests
{
    [Fact]
    public async Task RatingsAggregateOnlyVisibleReviewsAndDoNotExposeHiddenScores()
    {
        await using SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        ServiceCollection services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["REVIEWNOTIFICATION:DbConnectionString"] = "Host=unused;Database=unused"
        }).Build();
        ReviewNotificationModule.Presentation.ModuleRegistration.ModuleRegistration.RegisterModule(services, configuration);
        services.RemoveAll<DbContextOptions<ReviewNotificationDbContext>>();
        services.RemoveAll<IDbContextOptionsConfiguration<ReviewNotificationDbContext>>();
        services.AddDbContext<ReviewNotificationDbContext>(options => options.UseSqlite(connection));
        await using ServiceProvider provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewNotificationDbContext>();
        await db.Database.ExecuteSqlRawAsync("CREATE TABLE Review (ProductId TEXT, Rating INTEGER, Status TEXT)");
        Guid product = Guid.NewGuid();
        Guid hiddenOnly = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Review VALUES ({product}, 4, 'VISIBLE'), ({product}, 2, 'VISIBLE'), ({product}, 5, 'HIDDEN'), ({product}, 5, 'DELETED'), ({hiddenOnly}, 5, 'HIDDEN')");
        var ratings = await scope.ServiceProvider.GetRequiredService<IProductRatings>().GetRatingsAsync(CancellationToken.None);
        var rating = Assert.Single(ratings);
        Assert.Equal(product, rating.ProductId);
        Assert.Equal(3d, rating.AverageRating);
        Assert.Equal(2, rating.ReviewCount);
    }
}
