using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromotionModule.Application.Constants;
using PromotionModule.Application.Messages;
using PromotionModule.Presentation.Authorization;
using PromotionModule.Presentation.DTOs;
using PromotionModule.Application.UseCases.CreateVoucher;
using PromotionModule.Application.UseCases.UpdateVoucher;
using PromotionModule.Application.UseCases.GetVouchersForCustomer;
using PromotionModule.Application.UseCases.GetVoucher;
using PromotionModule.Application.UseCases.DeactivateVoucher;

namespace PromotionModule.Presentation.Controllers;

[ApiController]
[Route("api/vouchers")]
[AllowAnonymous]
public sealed class VoucherController : ControllerBase
{
    private readonly ISender _sender;

    public VoucherController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetVouchersForCustomer_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVouchers([FromQuery] int pageNumber = PaginationLimits.DefaultPageNumber, [FromQuery] int pageSize = PaginationLimits.DefaultPageSize, CancellationToken cancellationToken = default)
    {
        GetVouchersForCustomer_UCInput input = new GetVouchersForCustomer_UCInput(pageNumber, pageSize);
        GetVouchersForCustomer_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<GetVouchersForCustomer_UCOutput> response = new ApiSuccessResponse<GetVouchersForCustomer_UCOutput>(output, ResponseMessage.Listed);
        return Ok(response);
    }

    //[HttpGet("{id:guid}")]
    //[ProducesResponseType(typeof(ApiSuccessResponse<GetVoucherForCustomer_UCOutput>), StatusCodes.Status200OK)]
    //public async Task<IActionResult> GetVoucher(Guid id, CancellationToken cancellationToken)
    //{
    //    GetVoucherForCustomer_UCInput input = new GetVoucherForCustomer_UCInput(id);
    //    GetVoucherForCustomer_UCOutput output = await _sender.Send(input, cancellationToken);
    //    ApiSuccessResponse<GetVoucherForCustomer_UCOutput> response = new ApiSuccessResponse<GetVoucherForCustomer_UCOutput>(output, ResponseMessage.Retrieved);
    //    return Ok(response);
    //}

    //[Authorize(Roles = RoleAccess.Administrators)]
    //[HttpPost]
    //[ProducesResponseType(typeof(ApiSuccessResponse<CreateVoucher_UCOutput>), StatusCodes.Status201Created)]
    //public async Task<IActionResult> CreateVoucher([FromBody] VoucherRequest request, CancellationToken cancellationToken)
    //{
    //    CreateVoucher_UCInput input = new CreateVoucher_UCInput(request.Code, request.Type, request.Value, request.MaxUsage, request.MinOrderAmount, request.StartDate, request.EndDate);
    //    CreateVoucher_UCOutput output = await _sender.Send(input, cancellationToken);
    //    ApiSuccessResponse<CreateVoucher_UCOutput> response = new ApiSuccessResponse<CreateVoucher_UCOutput>(output, ResponseMessage.Created);
    //    return CreatedAtAction(nameof(GetVoucher), new { id = output.Id }, response);
    //}

    //[Authorize(Roles = RoleAccess.Administrators)]
    //[HttpPut("{id:guid}")]
    //[ProducesResponseType(typeof(ApiSuccessResponse<UpdateVoucher_UCOutput>), StatusCodes.Status200OK)]
    //public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] VoucherRequest request, CancellationToken cancellationToken)
    //{
    //    UpdateVoucher_UCInput input = new UpdateVoucher_UCInput(id, request.Code, request.Type, request.Value, request.MaxUsage, request.MinOrderAmount, request.StartDate, request.EndDate);
    //    UpdateVoucher_UCOutput output = await _sender.Send(input, cancellationToken);
    //    ApiSuccessResponse<UpdateVoucher_UCOutput> response = new ApiSuccessResponse<UpdateVoucher_UCOutput>(output, ResponseMessage.Updated);
    //    return Ok(response);
    //}

    //[Authorize(Roles = RoleAccess.Administrators)]
    //[HttpDelete("{id:guid}")]
    //[ProducesResponseType(typeof(ApiSuccessResponse<DeactivateVoucher_UCOutput>), StatusCodes.Status200OK)]
    //public async Task<IActionResult> DeactivateVoucher(Guid id, CancellationToken cancellationToken)
    //{
    //    DeactivateVoucher_UCInput input = new DeactivateVoucher_UCInput(id);
    //    DeactivateVoucher_UCOutput output = await _sender.Send(input, cancellationToken);
    //    ApiSuccessResponse<DeactivateVoucher_UCOutput> response = new ApiSuccessResponse<DeactivateVoucher_UCOutput>(output, ResponseMessage.Deactivated);
    //    return Ok(response);
    //}
}
