using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TechSalesManagement.Application.Common.Configurations;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Application.Services.Strategies.PaymentStrategies;

public class MomoPaymentStrategy : IPaymentStrategy
{
    private readonly MomoCO _momoConfig;
    private readonly IHttpClientFactory _httpClientFactory;

    public MomoPaymentStrategy(IOptions<MomoCO> momoConfig, IHttpClientFactory httpClientFactory)
    {
        _momoConfig = momoConfig.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Order order)
    {
        var partnerCode = string.IsNullOrEmpty(_momoConfig.PartnerCode) ? "MOCK_PARTNER_CODE" : _momoConfig.PartnerCode;
        var accessKey = string.IsNullOrEmpty(_momoConfig.AccessKey) ? "MOCK_ACCESS_KEY" : _momoConfig.AccessKey;
        var secretKey = string.IsNullOrEmpty(_momoConfig.SecretKey) ? "MOCK_SECRET_KEY" : _momoConfig.SecretKey;
        var endpoint = string.IsNullOrEmpty(_momoConfig.Endpoint) ? "https://test-payment.momo.vn/v2/gateway/api/create" : _momoConfig.Endpoint;
        var returnUrl = string.IsNullOrEmpty(_momoConfig.ReturnUrl) ? "http://localhost:3000/checkout/success" : _momoConfig.ReturnUrl;
        var notifyUrl = string.IsNullOrEmpty(_momoConfig.NotifyUrl) ? "https://your-domain.com/api/payment/momo-ipn" : _momoConfig.NotifyUrl;

        var orderId = order.id.ToString() + "_" + Guid.NewGuid().ToString().Substring(0, 8);
        var requestId = Guid.NewGuid().ToString();
        var amountStr = ((long)order.totalAmount).ToString();
        var amountVal = (long)order.totalAmount;
        var orderInfo = $"Payment for order {orderId}";
        var requestType = "captureWallet";
        var extraData = "";

        var rawHash = "accessKey=" + accessKey +
            "&amount=" + amountStr +
            "&extraData=" + extraData +
            "&ipnUrl=" + notifyUrl +
            "&orderId=" + orderId +
            "&orderInfo=" + orderInfo +
            "&partnerCode=" + partnerCode +
            "&redirectUrl=" + returnUrl +
            "&requestId=" + requestId +
            "&requestType=" + requestType;

        var signature = ComputeHmacSha256(rawHash, secretKey);

        var requestData = new
        {
            partnerCode = partnerCode,
            partnerName = "TechSales",
            storeId = "TechSalesStore",
            requestId = requestId,
            amount = amountVal,
            orderId = orderId,
            orderInfo = orderInfo,
            redirectUrl = returnUrl,
            ipnUrl = notifyUrl,
            lang = "vi",
            extraData = extraData,
            requestType = requestType,
            signature = signature
        };

        var client = _httpClientFactory.CreateClient();
        var jsonRequest = JsonSerializer.Serialize(requestData);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("payUrl", out var payUrlElement))
            {
                return new PaymentResult
                {
                    IsSuccess = true,
                    CheckoutUrl = payUrlElement.GetString(),
                    Message = "Redirecting to MoMo..."
                };
            }
            
            return new PaymentResult
            {
                IsSuccess = false,
                CheckoutUrl = null,
                Message = $"Failed to create MoMo payment. Response: {responseString}"
            };
        }
        catch (Exception ex)
        {
            return new PaymentResult
            {
                IsSuccess = false,
                CheckoutUrl = null,
                Message = $"Exception creating MoMo payment: {ex.Message}"
            };
        }
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
