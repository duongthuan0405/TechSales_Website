using CommonModule.Application.Messages;
using CommonModule.Presentation.ApiResponseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentRefundModule.Application.UseCases.GetTransactionMethodById;
using PaymentRefundModule.Application.UseCases.GetTransactionMethods;

namespace PaymentRefundModule.Presentation.Controllers;
[ApiController]
[Authorize]
[Route("api/admin/transaction-methods")]
public sealed class TransactionMethodController : ControllerBase
{
    private readonly ISender _sender;

    public TransactionMethodController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet()]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<GetTransactionMethods_UCOutput.TransactionMethodInfo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionMethods(CancellationToken ct = default)
    {
        GetTransactionMethods_UCOutput result = await _sender.Send(new GetTransactionMethods_UCInput(), ct);
        ApiSuccessResponse<List<GetTransactionMethods_UCOutput.TransactionMethodInfo>> response = new(result.TransactionMethods);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetTransactionMethodById_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionMethodById([FromRoute]Guid id, CancellationToken ct = default)
    {
        GetTransactionMethodById_UCOutput result = await _sender.Send(new GetTransactionMethodById_UCInput(id), ct);
        ApiSuccessResponse<GetTransactionMethodById_UCOutput> response = new(result);
        return Ok(response);
    }


}
