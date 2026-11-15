using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Common;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Implementations;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddToCartAsync(AddToCartParams parameters)
    {
        // BR63 / Quantity checks: Must be positive
        if (parameters.Quantity <= 0)
        {
            throw new BadRequestException(MessageConstants.MSG29);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            // 1. Get or create parent Cart
            var cart = await _cartRepository.GetByUserIdAsync(parameters.UserId);
            if (cart == null)
            {
                cart = new Cart
                {
                    id = Guid.NewGuid(),
                    userId = parameters.UserId,
                    createdAt = DateTimeOffset.UtcNow
                };
                await _cartRepository.AddCartAsync(cart);
            }

            // 2. Check existing items and add up requested quantity
            var existingItem = await _cartRepository.GetItemAsync(cart.id, parameters.ProductId);
            var totalRequestedQty = parameters.Quantity + (existingItem?.quantity ?? 0);

            // 3. Load product to validate stock (BR57)
            var product = await _productRepository.GetByIdAsync(parameters.ProductId);
            if (product == null)
            {
                throw new NotFoundException(MessageConstants.MSG25);
            }

            var availableQty = product.inventory?.availableQuantity ?? 0;
            if (totalRequestedQty > availableQty)
            {
                // BR58: Insufficient Stock => Throws MSG27
                throw new BadRequestException(MessageConstants.MSG27);
            }

            // 4. Write to relational database
            if (existingItem != null)
            {
                existingItem.quantity = totalRequestedQty;
                existingItem.updatedAt = DateTimeOffset.UtcNow;
                await _cartRepository.UpdateItemAsync(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    cartId = cart.id,
                    productId = parameters.ProductId,
                    quantity = parameters.Quantity,
                    createdAt = DateTimeOffset.UtcNow,
                    updatedAt = DateTimeOffset.UtcNow
                };
                await _cartRepository.AddItemAsync(newItem);
            }

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateCartItemAsync(UpdateCartItemParams parameters)
    {
        // BR63: Format Check => Must be positive
        if (parameters.Quantity <= 0)
        {
            throw new BadRequestException(MessageConstants.MSG29);
        }

        try
        {
            await _unitOfWork.BeginAsync();

            var cart = await _cartRepository.GetByUserIdAsync(parameters.UserId);
            if (cart == null)
            {
                throw new NotFoundException("Cart not found.");
            }

            var existingItem = await _cartRepository.GetItemAsync(cart.id, parameters.ProductId);
            if (existingItem == null)
            {
                throw new NotFoundException("Product not found in cart.");
            }

            // BR65: Stock Validation
            var product = await _productRepository.GetByIdAsync(parameters.ProductId);
            if (product == null)
            {
                throw new NotFoundException(MessageConstants.MSG25);
            }

            var availableQty = product.inventory?.availableQuantity ?? 0;
            if (parameters.Quantity > availableQty)
            {
                // BR66: Insufficient Stock => MSG27
                throw new BadRequestException(MessageConstants.MSG27);
            }

            // Update values
            existingItem.quantity = parameters.Quantity;
            existingItem.updatedAt = DateTimeOffset.UtcNow;
            await _cartRepository.UpdateItemAsync(existingItem);

            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task RemoveCartItemAsync(RemoveCartItemParams parameters)
    {
        try
        {
            await _unitOfWork.BeginAsync();

            var cart = await _cartRepository.GetByUserIdAsync(parameters.UserId);
            if (cart == null)
            {
                await _unitOfWork.FinishAsync();
                return;
            }

            await _cartRepository.RemoveItemAsync(cart.id, parameters.ProductId);
            
            await _unitOfWork.FinishAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Cart> GetCartAsync(GetCartParams parameters)
    {
        var cart = await _cartRepository.GetByUserIdAsync(parameters.UserId);
        if (cart == null)
        {
            return new Cart
            {
                userId = parameters.UserId,
                items = new List<CartItem>()
            };
        }
        return cart;
    }
}
