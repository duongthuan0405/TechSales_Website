using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentRefundModule.Application.UseCases.ReceiveMomoIpn;
using PaymentRefundModule.Presentation.DTOs;

namespace PaymentRefundModule.Presentation.Controllers;

[ApiController]
[Route("api/payments/momo")]
public sealed class MomoCallbackController : ControllerBase
{
    private readonly ISender _sender;

    public MomoCallbackController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("ipn")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReceiveIpn([FromBody] MomoIpnRequestDto request, CancellationToken cancellationToken)
    {
        ReceiveMomoIpn_UCInput input = new ReceiveMomoIpn_UCInput(
            request.PartnerCode, request.OrderId, request.RequestId, request.Amount,
            request.OrderInfo, request.OrderType, request.TransId, request.ResultCode,
            request.Message, request.PayType, request.ResponseTime, request.ExtraData, request.Signature);
        await _sender.Send(input, cancellationToken);
        return NoContent();
    }
}
