using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MediatR;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Application.UseCases.ProcessPaymentNotification;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.UseCases.ReceiveVnpayIpn;

internal sealed class ReceiveVnpayIpn_UCHandler : IRequestHandler<ReceiveVnpayIpn_UCInput, ReceiveVnpayIpn_UCOutput>
{
    private readonly VnpayOptions _options;
    private readonly IPaymentRepository _payments;
    private readonly ISender _sender;

    public ReceiveVnpayIpn_UCHandler(IOptions<VnpayOptions> options, IPaymentRepository payments, ISender sender)
    {
        _options = options.Value;
        _payments = payments;
        _sender = sender;
    }

    public async Task<ReceiveVnpayIpn_UCOutput> Handle(ReceiveVnpayIpn_UCInput request, CancellationToken cancellationToken)
    {
        Dictionary<string, string> fields = request.Parameters;
        if (!IsValid(fields))
        {
            return new ReceiveVnpayIpn_UCOutput("97", "Chữ ký thanh toán không hợp lệ.");
        }
        if (!fields.TryGetValue("vnp_TxnRef", out string? reference)
            || !Guid.TryParseExact(reference, "N", out Guid paymentId) || paymentId == Guid.Empty)
        {
            return new ReceiveVnpayIpn_UCOutput("01", "Không tìm thấy giao dịch thanh toán VNPAY.");
        }

        Payment? payment = await _payments.GetByIdAsync(paymentId, cancellationToken);
        if (payment == null || payment.TransactionMethod == null
            || !PaymentRules.MatchesGateway(payment.TransactionMethod.Type, payment.TransactionMethod.Code, "VNPAY"))
        {
            return new ReceiveVnpayIpn_UCOutput("01", "Không tìm thấy giao dịch thanh toán VNPAY.");
        }

        if (!fields.TryGetValue("vnp_Amount", out string? amountText)
            || !long.TryParse(amountText, NumberStyles.None, CultureInfo.InvariantCulture, out long scaledAmount)
            || !VnpayPaymentRules.MatchesCallbackAmount(payment.Amount, scaledAmount))
        {
            return new ReceiveVnpayIpn_UCOutput("04", "Số tiền thanh toán không khớp.");
        }
        if (!fields.TryGetValue("vnp_ResponseCode", out string? responseCode)
            || !fields.TryGetValue("vnp_TransactionStatus", out string? transactionStatus)
            || !fields.TryGetValue("vnp_TransactionNo", out string? transactionNo)
            || !long.TryParse(transactionNo, NumberStyles.None, CultureInfo.InvariantCulture, out long transactionId))
        {
            return new ReceiveVnpayIpn_UCOutput("99", "Chưa thể ghi nhận kết quả thanh toán.");
        }

        PaymentNotificationStatus status;
        if (VnpayPaymentRules.IsSuccessful(responseCode, transactionStatus) && transactionId > 0)
        {
            status = PaymentNotificationStatus.Success;
        }
        else if (VnpayPaymentRules.IsFailed(responseCode, transactionStatus))
        {
            status = PaymentNotificationStatus.Failed;
        }
        else
        {
            // Incomplete, conflicting, or refund statuses must not finalize a payment.
            return new ReceiveVnpayIpn_UCOutput("99", "Chưa thể ghi nhận kết quả thanh toán.");
        }

        if (payment.Status != TransactionStatus.PENDING)
        {
            if (payment.Status == TransactionStatus.SUCCESS
                && (status != PaymentNotificationStatus.Success || payment.TransactionRef != transactionNo))
            {
                return new ReceiveVnpayIpn_UCOutput("99", "Chưa thể ghi nhận kết quả thanh toán.");
            }
            return new ReceiveVnpayIpn_UCOutput("02", "Giao dịch đã được xử lý.");
        }

        ProcessPaymentNotification_UCOutput result = await _sender.Send(
            new ProcessPaymentNotification_UCInput(paymentId, payment.Amount, "VNPAY", transactionNo, status),
            cancellationToken);
        if (!result.Updated)
        {
            return new ReceiveVnpayIpn_UCOutput("02", "Giao dịch đã được xử lý.");
        }
        return new ReceiveVnpayIpn_UCOutput("00", "Đã ghi nhận kết quả thanh toán.");
    }

    internal bool IsValid(Dictionary<string, string> parameters)
    {
        if (string.IsNullOrWhiteSpace(_options.HashSecret)
            || string.IsNullOrWhiteSpace(_options.TmnCode)
            || !parameters.TryGetValue("vnp_TmnCode", out string? tmnCode)
            || !string.Equals(tmnCode, _options.TmnCode, StringComparison.Ordinal)
            || !parameters.TryGetValue("vnp_SecureHash", out string? signature)
            || signature.Length != 128)
        {
            return false;
        }

        string data = string.Join("&", parameters
            .Where(parameter => parameter.Key.StartsWith("vnp_", StringComparison.Ordinal)
                && parameter.Key != "vnp_SecureHash" && parameter.Key != "vnp_SecureHashType"
                && !string.IsNullOrEmpty(parameter.Value))
            .OrderBy(parameter => parameter.Key, StringComparer.Ordinal)
            .Select(parameter => WebUtility.UrlEncode(parameter.Key) + "=" + WebUtility.UrlEncode(parameter.Value)));
        byte[] expected = HMACSHA512.HashData(
            Encoding.UTF8.GetBytes(_options.HashSecret), Encoding.UTF8.GetBytes(data));
        try
        {
            return CryptographicOperations.FixedTimeEquals(expected, Convert.FromHexString(signature));
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
