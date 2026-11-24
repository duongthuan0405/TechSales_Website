using CartModule.Application.Repositories;
using CartModule.Domain.Entities;
using CartModule.Infrastructure.Persistence;
using CartModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CartModule.Infrastructure.Repositories;

internal sealed class CartRepository : ICartRepository
{
    private readonly CartDbContext _dbContext;

    public CartRepository(CartDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        CartDbModel? model = await _dbContext.Carts
            .AsNoTracking()
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);

        if (model == null)
        {
            return null;
        }

        return MapToDomain(model, model.Items);
    }

    public async Task<Cart?> GetByUserIdForUpdateAsync(Guid userId, CancellationToken cancellationToken)
    {
        CartDbModel? lockedCart = await _dbContext.Carts
            .FromSqlInterpolated($"""
                SELECT "Id", "UserId" FROM cart."Cart"
                WHERE "UserId" = {userId}
                FOR UPDATE
                """)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (lockedCart == null)
        {
            return null;
        }

        // Read the items after acquiring the lock to see changes committed while waiting.
        List<CartItemDbModel> items = await _dbContext.CartItems
            .AsNoTracking()
            .Where(item => item.CartId == lockedCart.Id)
            .ToListAsync(cancellationToken);

        return MapToDomain(lockedCart, items);
    }

    public async Task<bool> TryCreateAsync(Cart cart, CancellationToken cancellationToken)
    {
        int affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO cart."Cart" ("Id", "UserId") VALUES ({cart.Id}, {cart.UserId})
            ON CONFLICT ("UserId") DO NOTHING
            """,
            cancellationToken);

        return affectedRows == 1;
    }

    public async Task SaveItemAsync(CartItem item, CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO cart."CartItem" ("CartId", "ProductId", "Quantity", "CreatedAt", "UpdatedAt")
            VALUES ({item.CartId}, {item.ProductId}, {item.Quantity}, {item.CreatedAt}, {item.UpdatedAt})
            ON CONFLICT ("CartId", "ProductId") DO UPDATE
            SET "Quantity" = EXCLUDED."Quantity", "UpdatedAt" = EXCLUDED."UpdatedAt"
            """,
            cancellationToken);
    }

    public async Task<bool> UpdateItemAsync(CartItem item, CancellationToken cancellationToken)
    {
        int affectedRows = await _dbContext.CartItems
            .Where(row => row.CartId == item.CartId && row.ProductId == item.ProductId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(row => row.Quantity, item.Quantity)
                .SetProperty(row => row.UpdatedAt, item.UpdatedAt), cancellationToken);

        return affectedRows == 1;
    }

    public async Task RemoveItemAsync(Guid cartId, Guid productId, CancellationToken cancellationToken)
    {
        await _dbContext.CartItems
            .Where(row => row.CartId == cartId && row.ProductId == productId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static Cart MapToDomain(CartDbModel model, IEnumerable<CartItemDbModel> itemModels)
    {
        List<CartItem> items = itemModels
            .Select(item => new CartItem(item.CartId, item.ProductId, item.Quantity, item.CreatedAt, item.UpdatedAt))
            .ToList();

        return new Cart(model.Id, model.UserId, items);
    }
}
