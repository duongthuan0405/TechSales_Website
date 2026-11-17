using ShippingModule.Application.Messages;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShippingModule.Application.UseCases.GetProvinces;
using ShippingModule.Application.UseCases.GetWards;

namespace ShippingModule.Presentation.Controllers;

[ApiController]
[Route("api/shipping/provinces")]
[AllowAnonymous]
public sealed class LocationsController : ControllerBase
{
    private readonly ISender _sender;

    public LocationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetProvinces_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProvinces(CancellationToken cancellationToken)
    {
        GetProvinces_UCInput input = new GetProvinces_UCInput();
        GetProvinces_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<GetProvinces_UCOutput> response = new ApiSuccessResponse<GetProvinces_UCOutput>(
            output, ResponseMessage.SuccessProvinces);
        return Ok(response);
    }

    [HttpGet("{provinceCode}/wards")]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetWards_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWards(string provinceCode, CancellationToken cancellationToken)
    {
        GetWards_UCInput input = new GetWards_UCInput(provinceCode);
        GetWards_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<GetWards_UCOutput> response = new ApiSuccessResponse<GetWards_UCOutput>(
            output, ResponseMessage.SuccessWards);
        return Ok(response);
    }
}
