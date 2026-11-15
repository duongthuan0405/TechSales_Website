using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Infrastructure.Persistence;
using TechSalesManagement.Infrastructure.Persistence.Models;

namespace TechSalesManagement.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly TechSalesDbContext _dbContext;

    public CartRepository(TechSalesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        var dbModel = await _dbContext.Carts
            .Include(c => c.cart_items)
                .ThenInclude(ci => ci.product)
                    .ThenInclude(p => p.product_images)
            .Include(c => c.cart_items)
                .ThenInclude(ci => ci.product)
                    .ThenInclude(p => p.inventory)
            .FirstOrDefaultAsync(c => c.user_id == userId);

        return MapToEntity(dbModel);
    }

    public async Task AddCartAsync(Cart cart)
    {
        var dbModel = new CartDbModel
        {
            id = cart.id,
            user_id = cart.userId,
            created_at = cart.createdAt
        };

        await _dbContext.Carts.AddAsync(dbModel);
    }

    public async Task AddItemAsync(CartItem item)
    {
        var dbModel = new CartItemDbModel
        {
            cart_id = item.cartId,
            product_id = item.productId,
            quantity = item.quantity,
            created_at = item.createdAt,
            updated_at = item.updatedAt
        };

        await _dbContext.CartItems.AddAsync(dbModel);
    }

    public async Task UpdateItemAsync(CartItem item)
    {
        var dbModel = await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.cart_id == item.cartId && ci.product_id == item.productId);

        if (dbModel != null)
        {
            dbModel.quantity = item.quantity;
            dbModel.updated_at = DateTimeOffset.UtcNow;
            _dbContext.CartItems.Update(dbModel);
        }
    }

    public async Task RemoveItemAsync(Guid cartId, Guid productId)
    {
        var dbModel = await _dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.cart_id == cartId && ci.product_id == productId);

        if (dbModel != null)
        {
            _dbContext.CartItems.Remove(dbModel);
        }
    }

    public async Task<CartItem?> GetItemAsync(Guid cartId, Guid productId)
    {
        var dbModel = await _dbContext.CartItems
            .Include(ci => ci.product)
                .ThenInclude(p => p.inventory)
            .FirstOrDefaultAsync(ci => ci.cart_id == cartId && ci.product_id == productId);

        return MapItemToEntity(dbModel);
    }

    private Cart? MapToEntity(CartDbModel? dbModel)
    {
        if (dbModel == null) return null;

        var cart = new Cart
        {
            id = dbModel.id,
            userId = dbModel.user_id,
            createdAt = dbModel.created_at
        };

        if (dbModel.cart_items != null && dbModel.cart_items.Any())
        {
            cart.items = dbModel.cart_items.Select(MapItemToEntity).Cast<CartItem>().ToList();
        }

        return cart;
    }

    private CartItem? MapItemToEntity(CartItemDbModel? dbModel)
    {
        if (dbModel == null) return null;

        var item = new CartItem
        {
            cartId = dbModel.cart_id,
            productId = dbModel.product_id,
            quantity = dbModel.quantity,
            createdAt = dbModel.created_at,
            updatedAt = dbModel.updated_at
        };

        if (dbModel.product != null)
        {
            var product = new Product
            {
                id = dbModel.product.id,
                name = dbModel.product.name,
                description = dbModel.product.description,
                price = dbModel.product.price,
                status = dbModel.product.status,
                brand = dbModel.product.brand,
                categoryId = dbModel.product.category_id,
                createdAt = dbModel.product.created_at,
                updatedAt = dbModel.product.updated_at
            };

            if (dbModel.product.product_images != null)
            {
                product.images = dbModel.product.product_images.Select(img => new ProductImage
                {
                    id = img.id,
                    productId = img.product_id,
                    imageUrl = img.image_url,
                    isPrimary = img.is_primary,
                    createdAt = img.created_at
                }).ToList();
            }

            if (dbModel.product.inventory != null)
            {
                product.inventory = new Inventory
                {
                    productId = dbModel.product.inventory.product_id,
                    quantity = dbModel.product.inventory.quantity,
                    reservedQuantity = dbModel.product.inventory.reserved_quantity
                };
            }

            item.product = product;
        }

        return item;
    }
}
