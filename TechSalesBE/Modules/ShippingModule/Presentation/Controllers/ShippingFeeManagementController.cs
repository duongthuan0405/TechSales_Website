using ShippingModule.Application.Messages;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShippingModule.Application.UseCases.SetShippingFee;
using ShippingModule.Presentation.Authorization;
using ShippingModule.Presentation.DTOs;

namespace ShippingModule.Presentation.Controllers;

[ApiController]
[Route("api/shipping/provinces")]
[Authorize(Roles = RoleAccess.Administrators)]
public sealed class ShippingFeeManagementController : ControllerBase
{
    private readonly ISender _sender;

    public ShippingFeeManagementController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut("{provinceCode}/fee")]
    [ProducesResponseType(typeof(ApiSuccessResponse<SetShippingFee_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetFee(string provinceCode, [FromBody] SetShippingFeeRequest request,
        CancellationToken cancellationToken)
    {
        SetShippingFee_UCInput input = new SetShippingFee_UCInput(provinceCode, request.Fee);
        SetShippingFee_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<SetShippingFee_UCOutput> response = new ApiSuccessResponse<SetShippingFee_UCOutput>(
            output, ResponseMessage.SuccessShippingFeeSaved);
        return Ok(response);
    }
}
