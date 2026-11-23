using CommonModule.Presentation.ApiResponseModels;
using InventoryModule.Application.Constants;
using InventoryModule.Application.Messages;
using InventoryModule.Application.UseCases.ImportStock;
using InventoryModule.Application.UseCases.ReceiveStock;
using InventoryModule.Application.UseCases.UpdateProductUnitStatus;
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

    [HttpPost("~/api/inventory/import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(StockImportLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = StockImportLimits.MaxRequestBytes)]
    [ProducesResponseType(typeof(ApiSuccessResponse<ImportStock_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportStock([FromForm] ImportStockRequest request, CancellationToken cancellationToken)
    {
        IFormFile? file = request.File;
        await using Stream content = file?.OpenReadStream() ?? Stream.Null;
        ImportStock_UCInput input = new ImportStock_UCInput(content, file?.FileName ?? string.Empty, file?.Length ?? 0);
        ImportStock_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<ImportStock_UCOutput> response =
            new ApiSuccessResponse<ImportStock_UCOutput>(output, ResponseMessage.StockImported);
        return Ok(response);
    }

    [HttpPatch("units/status")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateProductUnitStatus_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProductUnitStatus(
        Guid productId, [FromBody] UpdateProductUnitStatusRequest request, CancellationToken cancellationToken)
    {
        UpdateProductUnitStatus_UCInput input =
            new UpdateProductUnitStatus_UCInput(productId, request.SerialNumber ?? string.Empty, request.Status ?? string.Empty);
        UpdateProductUnitStatus_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<UpdateProductUnitStatus_UCOutput> response =
            new ApiSuccessResponse<UpdateProductUnitStatus_UCOutput>(output, ResponseMessage.ProductUnitStatusUpdated);
        return Ok(response);
    }
}
