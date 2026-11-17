using ShippingModule.Domain.Entities;

namespace ShippingModule.Application.Repositories;

internal interface IShippingAddressRepository
{
    Task LockByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<ShippingAddress>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<ShippingAddress?> GetActiveAsync(Guid id, CancellationToken cancellationToken);
    Task SaveAsync(ShippingAddress address, CancellationToken cancellationToken);
}
