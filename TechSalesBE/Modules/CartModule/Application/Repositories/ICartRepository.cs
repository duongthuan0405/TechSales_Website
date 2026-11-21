using CartModule.Domain.Entities;

namespace CartModule.Application.Repositories;

internal interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> TryCreateAsync(Cart cart, CancellationToken cancellationToken);
}
