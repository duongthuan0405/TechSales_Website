using CartModule.Application.Messages;
using CartModule.Application.UseCases.Carts.GetCart;
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
}
