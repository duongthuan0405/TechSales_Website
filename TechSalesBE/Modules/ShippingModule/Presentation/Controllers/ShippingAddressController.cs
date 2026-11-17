using ShippingModule.Application.Messages;
using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ShippingModule.Presentation.DTOs;
using ShippingModule.Application.UseCases.GetAddresses;
using ShippingModule.Application.UseCases.CreateAddress;
using ShippingModule.Application.UseCases.UpdateAddress;
using ShippingModule.Application.UseCases.SetDefaultAddress;
using ShippingModule.Application.UseCases.DeleteAddress;

namespace ShippingModule.Presentation.Controllers;

[ApiController]
[Route("api/shipping-address")]
[Authorize]
public sealed class ShippingAddressController : ControllerBase
{
    private readonly ISender _sender;

    public ShippingAddressController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetAddresses_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        GetAddresses_UCInput input = new GetAddresses_UCInput(this.GetActorId());
        GetAddresses_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<GetAddresses_UCOutput> response = new ApiSuccessResponse<GetAddresses_UCOutput>(
            output, ResponseMessage.SuccessAddresses);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<CreateAddress_UCOutput>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAddress([FromBody] AddressRequest request, CancellationToken cancellationToken)
    {
        CreateAddress_UCInput input = new CreateAddress_UCInput(this.GetActorId(), request.WardCode, request.Detail);
        CreateAddress_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<CreateAddress_UCOutput> response = new ApiSuccessResponse<CreateAddress_UCOutput>(
            output, ResponseMessage.SuccessAddressCreated);
        return Created("/api/shipping-address", response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateAddress_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] AddressRequest request, CancellationToken cancellationToken)
    {
        UpdateAddress_UCInput input = new UpdateAddress_UCInput(this.GetActorId(), id, request.WardCode, request.Detail);
        UpdateAddress_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<UpdateAddress_UCOutput> response = new ApiSuccessResponse<UpdateAddress_UCOutput>(
            output, ResponseMessage.SuccessAddressUpdated);
        return Ok(response);
    }

    [HttpPut("{id:guid}/default")]
    [ProducesResponseType(typeof(ApiSuccessResponse<SetDefaultAddress_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDefaultAddress(Guid id, CancellationToken cancellationToken)
    {
        SetDefaultAddress_UCInput input = new SetDefaultAddress_UCInput(this.GetActorId(), id);
        SetDefaultAddress_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<SetDefaultAddress_UCOutput> response = new ApiSuccessResponse<SetDefaultAddress_UCOutput>(
            output, ResponseMessage.SuccessDefaultAddressUpdated);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<DeleteAddress_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        DeleteAddress_UCInput input = new DeleteAddress_UCInput(this.GetActorId(), id);
        DeleteAddress_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<DeleteAddress_UCOutput> response = new ApiSuccessResponse<DeleteAddress_UCOutput>(
            output, ResponseMessage.SuccessAddressDeleted);
        return Ok(response);
    }
}
