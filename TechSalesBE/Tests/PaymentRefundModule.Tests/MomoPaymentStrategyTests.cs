using System.Net;
using System.Text;
using System.Text.Json;
using CommonModule.BusinessExceptions;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Infrastructure.Payments;
using PaymentRefundModule.Infrastructure.Payments.Momo;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class MomoPaymentStrategyTests
{
    [Fact]
    public async Task SendsSignedRequestAndReturnsPaymentUrl()
    {
        using StubHandler handler = new StubHandler(HttpStatusCode.OK, """{"resultCode":0,"payUrl":"https://test-payment.momo.vn/pay/test"}""");
        using HttpClient client = new HttpClient(handler) { BaseAddress = new Uri("https://test-payment.momo.vn/v2/gateway/api/") };
        MomoPaymentStrategy strategy = CreateStrategy(client);
        Guid paymentId = Guid.NewGuid();

        PaymentCheckoutResult result = await strategy.CheckoutAsync(new PaymentCheckoutRequest(paymentId, 10000));

        Assert.Equal("https://test-payment.momo.vn/pay/test", result.PaymentUrl);
        Assert.Equal("https://test-payment.momo.vn/v2/gateway/api/create", handler.RequestUri?.AbsoluteUri);
        using JsonDocument document = JsonDocument.Parse(handler.Body!);
        JsonElement body = document.RootElement;
        Assert.Equal(paymentId.ToString("N"), body.GetProperty("orderId").GetString());
        Assert.Equal(10000, body.GetProperty("amount").GetInt64());
        Assert.Equal("captureWallet", body.GetProperty("requestType").GetString());
        Assert.Equal("https://example.com/ipn", body.GetProperty("ipnUrl").GetString());
        Assert.Matches("^[0-9a-f]{64}$", body.GetProperty("signature").GetString()!);
        Assert.False(body.TryGetProperty("secretKey", out _));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadGateway, "{}")]
    [InlineData(HttpStatusCode.OK, "{\"resultCode\":1001}")]
    [InlineData(HttpStatusCode.OK, "{\"resultCode\":0}")]
    [InlineData(HttpStatusCode.OK, "{\"payUrl\":\"https://example.com\"}")]
    [InlineData(HttpStatusCode.OK, "null")]
    [InlineData(HttpStatusCode.OK, "invalid json")]
    public async Task RejectsFailedOrInvalidResponse(HttpStatusCode status, string response)
    {
        using StubHandler handler = new StubHandler(status, response);
        using HttpClient client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/") };

        await Assert.ThrowsAsync<BusinessException>(() => CreateStrategy(client).CheckoutAsync(new PaymentCheckoutRequest(Guid.NewGuid(), 10000)));
    }

    private static MomoPaymentStrategy CreateStrategy(HttpClient client)
    {
        return new MomoPaymentStrategy(client,
            Options.Create(new MomoOptions { PartnerCode = "TEST", AccessKey = "test-access", SecretKey = "test-secret", IpnUrl = "https://example.com/ipn", RedirectUrl = "https://example.com/result" }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MomoPaymentStrategy>.Instance);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _response;
        public Uri? RequestUri { get; private set; }
        public string? Body { get; private set; }

        public StubHandler(HttpStatusCode status, string response)
        {
            _status = status;
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            RequestUri = request.RequestUri;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(_status) { Content = new StringContent(_response, Encoding.UTF8, "application/json") };
        }
    }
}
