using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Application.UseCases.ProcessPaymentNotification;
using CommonModule.BusinessExceptions;
using PaymentRefundModule.Application.Exceptions;

using MediatR;

namespace PaymentRefundModule.Application.UseCases.ReceiveMomoIpn;

internal sealed class ReceiveMomoIpn_UCHandler : IRequestHandler<ReceiveMomoIpn_UCInput, ReceiveMomoIpn_UCOutput>
{
    private readonly MomoOptions _options;
    private readonly ISender _sender;

    public ReceiveMomoIpn_UCHandler(IOptions<MomoOptions> options, ISender sender)
    {
        _options = options.Value;
        _sender = sender;
    }

    public async Task<ReceiveMomoIpn_UCOutput> Handle(ReceiveMomoIpn_UCInput notification, CancellationToken cancellationToken)
    {
        Console.WriteLine("Chuẩn bị cập nhật Payment");
        if (!IsValid(notification)
            || !Guid.TryParseExact(notification.OrderId, "N", out Guid paymentId)
            || paymentId == Guid.Empty || notification.Amount <= 0 || notification.TransId <= 0
            || string.IsNullOrWhiteSpace(notification.RequestId))
        {
            throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, BusinessExceptionMessage.MomoIpnInvalid);
        }

        PaymentNotificationStatus status = PaymentNotificationStatus.Unresolved;
        if (notification.ResultCode == 0)
        {
            status = PaymentNotificationStatus.Success;
        }
        ProcessPaymentNotification_UCInput input = new ProcessPaymentNotification_UCInput(paymentId, notification.Amount, "MOMO",
            notification.TransId.ToString(CultureInfo.InvariantCulture), status);
        ProcessPaymentNotification_UCOutput result = await _sender.Send(input, cancellationToken);
        return new ReceiveMomoIpn_UCOutput(result.Updated);
    }

    internal bool IsValid(ReceiveMomoIpn_UCInput notification)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey)
            || string.IsNullOrWhiteSpace(_options.AccessKey)
            || string.IsNullOrWhiteSpace(_options.PartnerCode)
            || !string.Equals(notification.PartnerCode, _options.PartnerCode, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(notification.Signature)
            || notification.Signature.Length != 64)
        {
            return false;
        }

        string rawData = $"accessKey={_options.AccessKey}"
            + $"&amount={notification.Amount.ToString(CultureInfo.InvariantCulture)}"
            + $"&extraData={notification.ExtraData}"
            + $"&message={notification.Message}"
            + $"&orderId={notification.OrderId}"
            + $"&orderInfo={notification.OrderInfo}"
            + $"&orderType={notification.OrderType}"
            + $"&partnerCode={notification.PartnerCode}"
            + $"&payType={notification.PayType}"
            + $"&requestId={notification.RequestId}"
            + $"&responseTime={notification.ResponseTime.ToString(CultureInfo.InvariantCulture)}"
            + $"&resultCode={notification.ResultCode.ToString(CultureInfo.InvariantCulture)}"
            + $"&transId={notification.TransId.ToString(CultureInfo.InvariantCulture)}";

        byte[] expected = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_options.SecretKey), Encoding.UTF8.GetBytes(rawData));
        try
        {
            byte[] actual = Convert.FromHexString(notification.Signature);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
