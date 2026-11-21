using CommonModule.Presentation.ApiResponseModels;
using InventoryModule.Application.Messages;
using InventoryModule.Application.UseCases.ReceiveStock;
using InventoryModule.Presentation.Authorization;
using InventoryModule.Presentation.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryModule.Presentation.Controllers;

[ApiController]
[Route("api/inventory/{productId:guid}")]
[Authorize(Roles = RoleAccess.StockManagers)]
public sealed class InventoryController : ControllerBase
{
    private readonly ISender _sender;

    public InventoryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("receive")]
    [ProducesResponseType(typeof(ApiSuccessResponse<ReceiveStock_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReceiveStock(
        Guid productId, [FromBody] ReceiveStockRequest request, CancellationToken cancellationToken)
    {
        ReceiveStock_UCInput input = new ReceiveStock_UCInput(productId, request.SerialNumbers ?? []);
        ReceiveStock_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<ReceiveStock_UCOutput> response =
            new ApiSuccessResponse<ReceiveStock_UCOutput>(output, ResponseMessage.StockReceived);
        return Ok(response);
    }
}
