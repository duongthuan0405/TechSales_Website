using System.Text.Json.Serialization;

namespace PaymentRefundModule.Infrastructure.Payments.Momo.Models;

internal sealed class MomoCreatePaymentResponse
{
    [JsonPropertyName("resultCode")]
    public int? ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("payUrl")]
    public string? PayUrl { get; set; }
}
