namespace PaymentRefundModule.Infrastructure.Payments.Options;

internal sealed class HookBackOptions
{
    public const string SectionName = "PAYMENTREFUND:HookBack";

    public string RedirectUrl { get; set; } = string.Empty;
}
