using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiSuccessResponse<CartResponseDto>>> GetCartAsync()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new GetCartParams { UserId = userId.Value };
        var cart = await _cartService.GetCartAsync(parameters);

        return Ok(new ApiSuccessResponse<CartResponseDto>(MapToDto(cart)));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> AddToCartAsync([FromBody] AddToCartRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new AddToCartParams
        {
            UserId = userId.Value,
            ProductId = request.productId,
            Quantity = request.quantity
        };

        await _cartService.AddToCartAsync(parameters);

        // BR60: Success notification MSG28
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG28));
    }

    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<ApiSuccessResponse<CartResponseDto>>> UpdateCartItemAsync(Guid productId, [FromBody] UpdateCartItemRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new UpdateCartItemParams
        {
            UserId = userId.Value,
            ProductId = productId,
            Quantity = request.quantity
        };

        await _cartService.UpdateCartItemAsync(parameters);

        // BR67: Returns updated cart data and status
        var updatedCart = await _cartService.GetCartAsync(new GetCartParams { UserId = userId.Value });
        return Ok(new ApiSuccessResponse<CartResponseDto>(MapToDto(updatedCart)));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> RemoveCartItemAsync(Guid productId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new RemoveCartItemParams
        {
            UserId = userId.Value,
            ProductId = productId
        };

        await _cartService.RemoveCartItemAsync(parameters);

        // BR70: Returns MSG31
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG31));
    }

    private CartResponseDto MapToDto(Cart cart)
    {
        return new CartResponseDto
        {
            userId = cart.userId,
            totalPrice = cart.totalPrice,
            totalItemsCount = cart.totalItemsCount,
            items = cart.items.Select(item => new CartItemResponseDto
            {
                productId = item.productId,
                quantity = item.quantity,
                createdAt = item.createdAt,
                updatedAt = item.updatedAt,
                product = item.product != null ? new CartProductResponseDto
                {
                    id = item.product.id,
                    name = item.product.name,
                    brand = item.product.brand,
                    price = item.product.price,
                    images = item.product.images.Select(img => new ProductImageResponseDto
                    {
                        id = img.id,
                        imageUrl = img.imageUrl,
                        isPrimary = img.isPrimary
                    }).ToList()
                } : null
            }).ToList()
        };
    }
}
