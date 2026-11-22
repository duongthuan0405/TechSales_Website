using ProductModule.Application.Ports;
using ProductModule.Domain.ValueObjects;

namespace TechSalesBE.WebAPI.Integration;

// Temporary adapter until the Brand/Category branches expose their public contracts.
// IDs match existing development FK rows. This adapter never reads or writes those tables.
internal sealed class MockProductReferenceData : IProductReferenceData
{
    public Task<List<ProductReference>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new List<ProductReference>
        {
            new ProductReference(Guid.Parse("0576832b-198e-4009-bc08-b824cdc7f983"), "Samsung Electronics"),
            new ProductReference(Guid.Parse("92adb9f5-9e27-4591-8a86-f8814d33e961"), "string")
        });
    }

    public Task<List<ProductReference>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new List<ProductReference>
        {
            new ProductReference(Guid.Parse("37b112dc-293f-4008-83a0-80128433b5d3"), "Headphone"),
            new ProductReference(Guid.Parse("a4412940-640f-4264-a361-32ca1059b388"), "Keyboard"),
            new ProductReference(Guid.Parse("78e7583d-a25e-4850-84f0-ca88e53952e6"), "Laptop"),
            new ProductReference(Guid.Parse("040ada2e-b952-4765-a4dc-01e85c387254"), "Mouse")
        });
    }
}
