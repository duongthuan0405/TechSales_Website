using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechSalesManagement.Application.Common.Configurations;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly MomoCO _momoConfig;
    private readonly IOrderService _orderService;

    public PaymentController(IOptions<MomoCO> momoConfig, IOrderService orderService)
    {
        _momoConfig = momoConfig.Value;
        _orderService = orderService;
    }

    [HttpPost("momo-ipn")]
    public async Task<IActionResult> MomoIpn([FromBody] MomoIpnRequestDto request)
    {
        // 1. Verify signature
        var rawHash = "accessKey=" + _momoConfig.AccessKey +
            "&amount=" + request.amount +
            "&extraData=" + request.extraData +
            "&message=" + request.message +
            "&orderId=" + request.orderId +
            "&orderInfo=" + request.orderInfo +
            "&orderType=" + request.orderType +
            "&partnerCode=" + request.partnerCode +
            "&payType=" + request.payType +
            "&requestId=" + request.requestId +
            "&responseTime=" + request.responseTime +
            "&resultCode=" + request.resultCode +
            "&transId=" + request.transId;

        var expectedSignature = ComputeHmacSha256(rawHash, _momoConfig.SecretKey);

        if (expectedSignature != request.signature)
        {
            return BadRequest(new { message = "Invalid signature" });
        }

        // 2. Process
        var orderIdStr = request.orderId;
        if (orderIdStr.Contains("_"))
        {
            orderIdStr = orderIdStr.Split('_')[0];
        }

        if (Guid.TryParse(orderIdStr, out var orderGuid))
        {
            await _orderService.HandlePaymentIpnAsync(orderGuid, request.transId.ToString(), request.resultCode);
        }

        return NoContent();
    }

    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
