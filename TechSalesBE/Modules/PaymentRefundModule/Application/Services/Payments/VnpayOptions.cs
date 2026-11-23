namespace PaymentRefundModule.Application.Services.Payments;

internal sealed class VnpayOptions
{
    public const string SectionName = "PAYMENTREFUND:VNPAY";

    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 15;

    // Registered with VNPAY separately; not a parameter of the checkout URL.
    public string IpnUrl { get; set; } = string.Empty;
}
