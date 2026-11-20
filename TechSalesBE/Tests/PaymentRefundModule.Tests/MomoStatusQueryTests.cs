using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Infrastructure.Payments.Momo;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class MomoStatusQueryTests
{
    [Theory]
    [InlineData("valid", true)]
    [InlineData("order", false)]
    [InlineData("partner", false)]
    [InlineData("request", false)]
    [InlineData("missing-code", false)]
    [InlineData("invalid-trans", false)]
    public async Task SignsQueryAndRejectsUnmatchedOrIncompleteSuccess(string scenario, bool valid)
    {
        using Handler handler = new Handler(scenario);
        using HttpClient client = new HttpClient(handler) { BaseAddress = new Uri("https://example.com/v2/gateway/api/") };
        MomoStatusQuery query = new MomoStatusQuery(client,
            Options.Create(new MomoOptions { PartnerCode = "TEST", AccessKey = "access", SecretKey = "secret" }),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MomoStatusQuery>.Instance);
        Guid id = Guid.NewGuid();
        if (valid)
        {
            MomoStatusResult result = await query.QueryAsync(id, default);
            Assert.Equal(id.ToString("N"), result.OrderId);
            Assert.Equal(10000, result.Amount);
            Assert.Equal(123456, result.TransId);
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(() => query.QueryAsync(id, default));
        }
    }

    private sealed class Handler : HttpMessageHandler
    {
        private readonly string _scenario;
        public Handler(string scenario)
        {
            _scenario = scenario;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/v2/gateway/api/query", request.RequestUri!.AbsolutePath);
            using JsonDocument json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            JsonElement body = json.RootElement;
            string orderId = body.GetProperty("orderId").GetString()!;
            string requestId = body.GetProperty("requestId").GetString()!;
            string raw = $"accessKey=access&orderId={orderId}&partnerCode=TEST&requestId={requestId}";
            string expected = Convert.ToHexStringLower(HMACSHA256.HashData(Encoding.UTF8.GetBytes("secret"), Encoding.UTF8.GetBytes(raw)));
            Assert.Equal(expected, body.GetProperty("signature").GetString());
            Dictionary<string, object> response = new Dictionary<string, object>
            {
                ["partnerCode"] = _scenario == "partner" ? "OTHER" : "TEST",
                ["orderId"] = _scenario == "order" ? "OTHER" : orderId,
                ["requestId"] = _scenario == "request" ? "OTHER" : requestId,
                ["amount"] = 10000,
                ["transId"] = _scenario == "invalid-trans" ? 0 : 123456
            };
            if (_scenario != "missing-code")
            {
                response["resultCode"] = 0;
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(response), Encoding.UTF8, "application/json")
            };
        }
    }
}
