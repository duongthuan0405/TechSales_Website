using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentRefundModule.Application.Constants;
using PaymentRefundModule.Application.UseCases.GetPaymentsForStaff;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Presentation.Authorization;

namespace PaymentRefundModule.Presentation.Controllers;

[ApiController]
[Authorize(Roles = RoleAccess.PaymentManagement)]
[Route("api/admin/payments")]
public sealed class AdminPaymentController : ControllerBase
{
    private readonly ISender _sender;

    public AdminPaymentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetPaymentsForStaff_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int pageNumber = PaginationLimits.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationLimits.DefaultPageSize,
        [FromQuery] string? type = null, [FromQuery] string? status = null,
        [FromQuery] Guid? refId = null, CancellationToken cancellationToken = default)
    {
        GetPaymentsForStaff_UCOutput output = await _sender.Send(
            new GetPaymentsForStaff_UCInput(pageNumber, pageSize, type, status, refId), cancellationToken);
        return Ok(new ApiSuccessResponse<GetPaymentsForStaff_UCOutput>(output));
    }
}
