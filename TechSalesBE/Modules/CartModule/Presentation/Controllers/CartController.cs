using CartModule.Application.Messages;
using CartModule.Application.UseCases.Carts.AddCartItem;
using CartModule.Application.UseCases.Carts.GetCart;
using CartModule.Application.UseCases.Carts.RemoveCartItem;
using CartModule.Application.UseCases.Carts.UpdateCartItem;
using CartModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CartModule.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public sealed class CartController : ControllerBase
{
    private readonly ISender _sender;

    public CartController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetCart_UCOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        GetCart_UCInput input = new GetCart_UCInput(this.GetActorId());
        GetCart_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<GetCart_UCOutput> response = new ApiSuccessResponse<GetCart_UCOutput>(
            output, ResponseMessage.CartRetrieved);
        return Ok(response);
    }

    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiSuccessResponse<AddCartItem_UCOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddCartItem([FromBody] AddCartItemRequest request, CancellationToken cancellationToken)
    {
        AddCartItem_UCInput input = new AddCartItem_UCInput(this.GetActorId(), request.ProductId, request.Quantity);
        AddCartItem_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<AddCartItem_UCOutput> response = new ApiSuccessResponse<AddCartItem_UCOutput>(
            output, ResponseMessage.ItemAdded);
        return Ok(response);
    }

    [HttpPut("items/{productId}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateCartItem_UCOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateCartItem(
        [FromRoute] Guid productId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        UpdateCartItem_UCInput input = new UpdateCartItem_UCInput(this.GetActorId(), productId, request.Quantity);
        UpdateCartItem_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<UpdateCartItem_UCOutput> response = new ApiSuccessResponse<UpdateCartItem_UCOutput>(
            output, ResponseMessage.ItemQuantityUpdated);
        return Ok(response);
    }

    [HttpDelete("items/{productId}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<RemoveCartItem_UCOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RemoveCartItem([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        RemoveCartItem_UCInput input = new RemoveCartItem_UCInput(this.GetActorId(), productId);
        RemoveCartItem_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<RemoveCartItem_UCOutput> response = new ApiSuccessResponse<RemoveCartItem_UCOutput>(
            output, ResponseMessage.ItemRemoved);
        return Ok(response);
    }
}
