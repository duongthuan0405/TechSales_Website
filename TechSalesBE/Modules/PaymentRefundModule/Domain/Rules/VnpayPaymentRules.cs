namespace PaymentRefundModule.Domain.Rules;

internal static class VnpayPaymentRules
{
    // VNPAY accepts at most 12 digits after multiplying the VND amount by 100.
    public const long MaximumAmount = 9_999_999_999;

    public static bool HasValidAmount(long amount)
    {
        return amount > 0 && amount <= MaximumAmount;
    }

    public static bool MatchesCallbackAmount(long expectedAmount, long receivedAmount)
    {
        return HasValidAmount(expectedAmount) && receivedAmount == expectedAmount * 100;
    }

    public static bool IsSuccessful(string responseCode, string status)
    {
        return responseCode == "00" && status == "00";
    }

    public static bool IsFailed(string responseCode, string status)
    {
        return responseCode.Length == 2 && responseCode.All(char.IsAsciiDigit) && responseCode != "00"
            && (status == "02" || status == "04" || status == "07");
    }
}
