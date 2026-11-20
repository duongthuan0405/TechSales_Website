using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CommonModule.BusinessExceptions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Infrastructure.Payments.Vnpay;
using Xunit;

namespace PaymentRefundModule.Tests;

public sealed class VnpayPaymentStrategyTests
{
    [Fact]
    public async Task CreatesSignedUrlWithVndAmountPaymentReferenceAndVietnamTime()
    {
        Guid paymentId = Guid.NewGuid();
        DateTime before = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
        PaymentCheckoutResult result = await CreateStrategy().CheckoutAsync(
            new PaymentCheckoutRequest(paymentId, 100000, "::ffff:192.0.2.10"));
        DateTime after = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime;
        Uri url = new Uri(result.PaymentUrl);
        var parameters = QueryHelpers.ParseQuery(url.Query);

        Assert.Equal("sandbox.vnpayment.vn", url.Host);
        Assert.Equal("/paymentv2/vpcpay.html", url.AbsolutePath);
        Assert.Equal("10000000", parameters["vnp_Amount"].ToString());
        Assert.Equal(paymentId.ToString("N"), parameters["vnp_TxnRef"].ToString());
        Assert.Equal("192.0.2.10", parameters["vnp_IpAddr"].ToString());
        Assert.Equal("https://example.com/return?source=test&text=a b+c", parameters["vnp_ReturnUrl"].ToString());
        Assert.Equal("VND", parameters["vnp_CurrCode"].ToString());
        Assert.Equal("2.1.0", parameters["vnp_Version"].ToString());
        DateTime created = DateTime.ParseExact(parameters["vnp_CreateDate"].ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        DateTime expires = DateTime.ParseExact(parameters["vnp_ExpireDate"].ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        Assert.InRange(created, before.AddSeconds(-1), after);
        Assert.Equal(TimeSpan.FromMinutes(15), expires - created);

        string rawQuery = url.Query.TrimStart('?');
        int signatureStart = rawQuery.LastIndexOf("&vnp_SecureHash=", StringComparison.Ordinal);
        string signedQuery = rawQuery.Substring(0, signatureStart);
        string[] keys = signedQuery.Split('&').Select(pair => pair.Split('=')[0]).ToArray();
        Assert.Equal(keys.OrderBy(key => key, StringComparer.Ordinal), keys);
        Assert.Contains("Thanh+toan+giao+dich+", signedQuery);
        Assert.Contains("%26text%3Da+b%2Bc", signedQuery);
        string expectedSignature = Convert.ToHexStringLower(HMACSHA512.HashData(
            Encoding.UTF8.GetBytes("test-secret"), Encoding.UTF8.GetBytes(signedQuery)));
        Assert.Equal(expectedSignature, parameters["vnp_SecureHash"].ToString());
        Assert.DoesNotContain("test-secret", result.PaymentUrl);
        Assert.False(parameters.ContainsKey("vnp_IpnUrl"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10000000000)]
    [InlineData(long.MaxValue)]
    public async Task RejectsInvalidOrOverflowingAmount(long amount)
    {
        await Assert.ThrowsAsync<BusinessException>(() => CreateStrategy().CheckoutAsync(
            new PaymentCheckoutRequest(Guid.NewGuid(), amount, "127.0.0.1")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-an-ip")]
    public async Task RejectsMissingOrInvalidClientIp(string? ip)
    {
        await Assert.ThrowsAsync<BusinessException>(() => CreateStrategy().CheckoutAsync(
            new PaymentCheckoutRequest(Guid.NewGuid(), 10000, ip)));
    }

    [Fact]
    public async Task RejectsMissingCredentials()
    {
        VnpayPaymentStrategy strategy = new VnpayPaymentStrategy(Options.Create(new VnpayOptions()));
        await Assert.ThrowsAsync<BusinessException>(() => strategy.CheckoutAsync(
            new PaymentCheckoutRequest(Guid.NewGuid(), 10000, "127.0.0.1")));
    }

    private static VnpayPaymentStrategy CreateStrategy()
    {
        return new VnpayPaymentStrategy(Options.Create(new VnpayOptions
        {
            TmnCode = "TESTCODE",
            HashSecret = "test-secret",
            PaymentUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
            ReturnUrl = "https://example.com/return?source=test&text=a b+c"
        }));
    }
}
