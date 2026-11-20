namespace PaymentRefundModule.Application.Services.Payments;

internal sealed class MomoOptions
{
    public const string SectionName = "PAYMENTREFUND:MOMO";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string PartnerCode { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}
