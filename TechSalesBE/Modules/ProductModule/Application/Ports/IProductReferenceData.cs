using ProductModule.Domain.ValueObjects;

namespace ProductModule.Application.Ports;

// Product consumes these references; Brand and Category are owned elsewhere.
public interface IProductReferenceData
{
    Task<List<ProductReference>> GetBrandsAsync(CancellationToken cancellationToken);
    Task<List<ProductReference>> GetCategoriesAsync(CancellationToken cancellationToken);
}
