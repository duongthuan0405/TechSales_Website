using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PaymentRefundModule.Application.Services.Payments;

namespace PaymentRefundModule.Infrastructure.Payments.Momo;

internal sealed class MomoStatusQuery : IMomoStatusQuery
{
    private readonly HttpClient _client;
    private readonly MomoOptions _options;
    private readonly ILogger<MomoStatusQuery> _logger;

    public MomoStatusQuery(HttpClient client, IOptions<MomoOptions> options, ILogger<MomoStatusQuery> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MomoStatusResult> QueryAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        string orderId = paymentId.ToString("N");
        string requestId = Guid.NewGuid().ToString("N");
        string raw = $"accessKey={_options.AccessKey}&orderId={orderId}&partnerCode={_options.PartnerCode}&requestId={requestId}";
        string signature = Convert.ToHexStringLower(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_options.SecretKey), Encoding.UTF8.GetBytes(raw)));
        using HttpResponseMessage response = await _client.PostAsJsonAsync("query", new
        {
            partnerCode = _options.PartnerCode,
            requestId,
            orderId,
            lang = "vi",
            signature
        }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            const int maxLoggedLength = 2000;
            if (errorBody.Length > maxLoggedLength)
            {
                errorBody = errorBody.Substring(0, maxLoggedLength);
            }
            _logger.LogWarning(
                "MoMo query HTTP error. OrderId: {OrderId}, RequestId: {RequestId}, StatusCode: {StatusCode}, ContentType: {ContentType}, Server: {Server}, ResponseBody: {ResponseBody}",
                orderId, requestId, (int)response.StatusCode,
                response.Content.Headers.ContentType?.ToString(), response.Headers.Server.ToString(), errorBody);
            response.EnsureSuccessStatusCode();
        }
        using JsonDocument document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        JsonElement body = document.RootElement;
        int resultCode = body.GetProperty("resultCode").GetInt32();
        // Errors such as an order not yet created must never be treated as success.
        if (resultCode != 0)
        {
            return new MomoStatusResult(orderId, 0, 0, resultCode);
        }
        if (body.GetProperty("partnerCode").GetString() != _options.PartnerCode ||
            body.GetProperty("orderId").GetString() != orderId ||
            body.GetProperty("requestId").GetString() != requestId)
        {
            throw new InvalidOperationException("MoMo query response does not match the request.");
        }

        long amount = body.GetProperty("amount").GetInt64();
        long transId = body.GetProperty("transId").GetInt64();
        if (amount <= 0 || transId <= 0)
        {
            throw new InvalidOperationException("MoMo query response has invalid transaction data.");
        }
        return new MomoStatusResult(orderId, amount, transId, resultCode);
    }
}

