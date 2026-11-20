using System.Text.Json.Serialization;

namespace PaymentRefundModule.Presentation.DTOs;

public sealed record VnpayIpnResponseDto(
    [property: JsonPropertyName("RspCode")] string RspCode,
    [property: JsonPropertyName("Message")] string Message);
