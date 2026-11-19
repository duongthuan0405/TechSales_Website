namespace PaymentRefundModule.Presentation.DTOs;

public sealed class MomoIpnRequestDto
{
    public required string PartnerCode { get; set; }
    public required string OrderId { get; set; }
    public required string RequestId { get; set; }
    public required long Amount { get; set; }
    public required string OrderInfo { get; set; }
    public required string OrderType { get; set; }
    public required long TransId { get; set; }
    public required int ResultCode { get; set; }
    public required string Message { get; set; }
    public required string PayType { get; set; }
    public required long ResponseTime { get; set; }
    public required string ExtraData { get; set; }
    public required string Signature { get; set; }
}
