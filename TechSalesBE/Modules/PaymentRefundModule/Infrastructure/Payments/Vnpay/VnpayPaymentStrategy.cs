using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CommonModule.BusinessExceptions;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Exceptions;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Infrastructure.Payments.Vnpay;

internal sealed class VnpayPaymentStrategy : IPaymentGatewayStrategy
{
    private readonly VnpayOptions _options;

    public VnpayPaymentStrategy(IOptions<VnpayOptions> options)
    {
        _options = options.Value;
    }

    public string GatewayCode => "VNPAY";

    public Task<PaymentCheckoutResult> CheckoutAsync(
        PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(_options.TmnCode)
            || string.IsNullOrWhiteSpace(_options.HashSecret)
            || !Uri.TryCreate(_options.PaymentUrl, UriKind.Absolute, out Uri? paymentUrl)
            || paymentUrl.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(paymentUrl.Query) || !string.IsNullOrEmpty(paymentUrl.Fragment)
            || !Uri.TryCreate(_options.ReturnUrl, UriKind.Absolute, out Uri? returnUrl)
            || (returnUrl.Scheme != Uri.UriSchemeHttps && returnUrl.Scheme != Uri.UriSchemeHttp))
        {
            throw new BusinessException(BusinessExceptionCode.CONFLICT, BusinessExceptionMessage.PaymentGatewayNotConfigured);
        }
        if (request.PaymentId == Guid.Empty)
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentIdInvalid);
        }
        if (!VnpayPaymentRules.HasValidAmount(request.Amount))
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.VnpayAmountInvalid);
        }
        if (!IPAddress.TryParse(request.ClientIpAddress, out IPAddress? clientIp))
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.PaymentClientIpInvalid);
        }
        if (clientIp.IsIPv4MappedToIPv6)
        {
            clientIp = clientIp.MapToIPv4();
        }

        DateTimeOffset createdAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        SortedDictionary<string, string> parameters = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = checked(request.Amount * 100).ToString(CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = request.PaymentId.ToString("N"),
            ["vnp_OrderInfo"] = $"Thanh toan giao dich {request.PaymentId:N}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = _options.ReturnUrl,
            ["vnp_IpAddr"] = clientIp.ToString(),
            ["vnp_CreateDate"] = createdAt.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_ExpireDate"] = createdAt.AddMinutes(15).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)
        };

        // Sign exactly the sorted, form-encoded query sent to VNPAY (spaces are '+').
        string query = string.Join("&", parameters.Select(parameter =>
            WebUtility.UrlEncode(parameter.Key) + "=" + WebUtility.UrlEncode(parameter.Value)));
        string signature = Convert.ToHexStringLower(HMACSHA512.HashData(
            Encoding.UTF8.GetBytes(_options.HashSecret), Encoding.UTF8.GetBytes(query)));
        string url = _options.PaymentUrl + "?" + query + "&vnp_SecureHash=" + signature;
        return Task.FromResult(new PaymentCheckoutResult(url));
    }
}
