using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentRefundModule.Application.UseCases.CreatePayment;
using PaymentRefundModule.Application.UseCases.GetPaymentById;
using PaymentRefundModule.Application.UseCases.GetPayments;
using PaymentRefundModule.Application.UseCases.GetPaymentByRefId;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Presentation.DTOs;
using PaymentRefundModule.Presentation.Authorization;
using PaymentRefundModule.Application.Constants;

namespace PaymentRefundModule.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("api/payments")]
public sealed class PaymentController : ControllerBase
{
    private readonly ISender _sender;

    public PaymentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetPaymentById_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentById(Guid id, CancellationToken cancellationToken)
    {
        GetPaymentById_UCOutput output = await _sender.Send(
            new GetPaymentById_UCInput(this.GetActorId(), id, CanViewAllPayments()), cancellationToken);
        return Ok(new ApiSuccessResponse<GetPaymentById_UCOutput>(output));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetPayments_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int pageNumber = PaginationLimits.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationLimits.DefaultPageSize,
        [FromQuery] string? type = null, [FromQuery] string? status = null,
        [FromQuery] Guid? refId = null, CancellationToken cancellationToken = default)
    {
        GetPayments_UCOutput output = await _sender.Send(
            new GetPayments_UCInput(this.GetActorId(), pageNumber, pageSize, type, status, refId), cancellationToken);
        return Ok(new ApiSuccessResponse<GetPayments_UCOutput>(output));
    }

    [HttpGet("by-ref/{refId:guid}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetPaymentByRefId_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentByRefId(Guid refId, [FromQuery] string? type,
        [FromQuery] int pageNumber = PaginationLimits.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationLimits.DefaultPageSize,
        [FromQuery] string? status = null, CancellationToken cancellationToken = default)
    {
        GetPaymentByRefId_UCOutput output = await _sender.Send(
            new GetPaymentByRefId_UCInput(this.GetActorId(), refId, type, CanViewAllPayments(), pageNumber, pageSize, status), cancellationToken);
        return Ok(new ApiSuccessResponse<GetPaymentByRefId_UCOutput>(output));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<CreatePayment_UCOutput>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto request, CancellationToken cancellationToken)
    {
        CreatePayment_UCInput input = new CreatePayment_UCInput(
            this.GetActorId(), request.Type, request.RefId, request.TransactionMethodId);
        CreatePayment_UCOutput output = await _sender.Send(input, cancellationToken);
        ApiSuccessResponse<CreatePayment_UCOutput> response = new ApiSuccessResponse<CreatePayment_UCOutput>(output);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    private bool CanViewAllPayments()
    {
        return User.IsInRole(RoleAccess.Staff)
            || User.IsInRole(RoleAccess.BusinessAdmin)
            || User.IsInRole(RoleAccess.TechnicalAdmin);
    }
}
