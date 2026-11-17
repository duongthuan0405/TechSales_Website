using ShippingModule.Application.Messages;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShippingModule.Application.UseCases.GetShippingFee;

namespace ShippingModule.Presentation.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/shipping")]
public sealed class ShippingFeesController : ControllerBase
{
    private readonly ISender _sender;

    public ShippingFeesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("provinces/{provinceCode}/fee")]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetShippingFee_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProvince(string provinceCode, CancellationToken cancellationToken)
    {
        GetShippingFee_UCInput input = new GetShippingFee_UCInput(provinceCode, null);
        GetShippingFee_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<GetShippingFee_UCOutput> response = new ApiSuccessResponse<GetShippingFee_UCOutput>(
            output, ResponseMessage.SuccessShippingFee);
        return Ok(response);
    }

    [HttpGet("wards/{wardCode}/fee")]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetShippingFee_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByWard(string wardCode, CancellationToken cancellationToken)
    {
        GetShippingFee_UCInput input = new GetShippingFee_UCInput(null, wardCode);
        GetShippingFee_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<GetShippingFee_UCOutput> response = new ApiSuccessResponse<GetShippingFee_UCOutput>(
            output, ResponseMessage.SuccessShippingFee);
        return Ok(response);
    }
}
