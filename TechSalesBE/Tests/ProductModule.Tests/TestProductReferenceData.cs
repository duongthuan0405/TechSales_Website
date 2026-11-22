using ProductModule.Application.Ports;
using ProductModule.Domain.ValueObjects;

namespace ProductModule.Tests;

internal sealed class TestProductReferenceData : IProductReferenceData
{
    internal List<ProductReference> Brands { get; } = new List<ProductReference>();
    internal List<ProductReference> Categories { get; } = new List<ProductReference>();

    public Task<List<ProductReference>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Brands.ToList());
    }

    public Task<List<ProductReference>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Categories.ToList());
    }
}
