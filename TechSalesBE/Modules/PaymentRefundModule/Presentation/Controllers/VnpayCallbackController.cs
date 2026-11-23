using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentRefundModule.Application.UseCases.ReceiveVnpayIpn;
using PaymentRefundModule.Presentation.DTOs;

namespace PaymentRefundModule.Presentation.Controllers;

[ApiController]
[Route("api/payments/vnpay")]
public sealed class VnpayCallbackController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<VnpayCallbackController> _logger;

    public VnpayCallbackController(ISender sender, ILogger<VnpayCallbackController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("ipn")]
    [ProducesResponseType(typeof(VnpayIpnResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReceiveIpn(CancellationToken cancellationToken)
    {
        Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var parameter in Request.Query)
        {
            if (!parameter.Key.StartsWith("vnp_", StringComparison.Ordinal))
            {
                continue;
            }
            if (parameter.Value.Count != 1)
            {
                return Reply(new ReceiveVnpayIpn_UCOutput("97", "Chữ ký thanh toán không hợp lệ."));
            }
            fields.Add(parameter.Key, parameter.Value.ToString());
        }

        try
        {
            ReceiveVnpayIpn_UCOutput output = await _sender.Send(new ReceiveVnpayIpn_UCInput(fields), cancellationToken);
            _logger.LogInformation("VNPAY IPN processed. PaymentReference: {PaymentReference}, RspCode: {RspCode}",
                fields.GetValueOrDefault("vnp_TxnRef"), output.RspCode);
            return Reply(output);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "VNPAY IPN could not be committed. PaymentReference: {PaymentReference}",
                fields.GetValueOrDefault("vnp_TxnRef"));
            return Reply(new ReceiveVnpayIpn_UCOutput("99", "Chưa thể ghi nhận kết quả thanh toán."));
        }
    }

    private IActionResult Reply(ReceiveVnpayIpn_UCOutput output)
    {
        // VNPAY requires this protocol payload, without the application's usual API envelope.
        return Ok(new VnpayIpnResponseDto(output.RspCode, output.Message));
    }
}
