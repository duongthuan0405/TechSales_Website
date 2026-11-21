using CartModule.Domain.Entities;

namespace CartModule.Application.Repositories;

internal interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Cart?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> TryCreateAsync(Cart cart, CancellationToken cancellationToken);

    Task SaveItemAsync(CartItem item, CancellationToken cancellationToken);
}
