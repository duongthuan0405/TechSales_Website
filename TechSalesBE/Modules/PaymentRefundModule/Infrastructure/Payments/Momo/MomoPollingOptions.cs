namespace PaymentRefundModule.Infrastructure.Payments.Momo;

internal sealed class MomoPollingOptions
{
    public const string SectionName = "PAYMENTREFUND:MOMO:Polling";
    public bool Enabled { get; set; }
    public int IntervalSeconds { get; set; } = 30;
    public int BatchSize { get; set; } = 20;
    public int RequestSpacingMilliseconds { get; set; } = 1000;
}

