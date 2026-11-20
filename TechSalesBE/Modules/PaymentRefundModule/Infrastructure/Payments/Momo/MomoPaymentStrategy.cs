using PaymentRefundModule.Application.Services.Payments;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PaymentRefundModule.Infrastructure.Payments.Options;
using PaymentRefundModule.Infrastructure.Payments.Momo.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http.Json;
using System.Text.Json;
using CommonModule.BusinessExceptions;
using PaymentRefundModule.Application.Exceptions;

namespace PaymentRefundModule.Infrastructure.Payments.Momo;

internal sealed class MomoPaymentStrategy : IPaymentGatewayStrategy
{
    private readonly MomoOptions _momoOptions;
    private readonly HookBackOptions _hookBackOptions;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MomoPaymentStrategy> _logger;

    public MomoPaymentStrategy(
        HttpClient httpClient,
        IOptions<MomoOptions> momoOptions,
        IOptions<HookBackOptions> hookBackOptions,
        ILogger<MomoPaymentStrategy> logger)
    {
        _httpClient = httpClient;
        _momoOptions = momoOptions.Value;
        _hookBackOptions = hookBackOptions.Value;
        _logger = logger;
    }

    public string GatewayCode => "MOMO";

    private MomoCreatePaymentRequest BuildCreatePaymentRequest(PaymentCheckoutRequest request)
    {
        MomoCreatePaymentRequest momoRequest = new MomoCreatePaymentRequest
        {
            PartnerCode = _momoOptions.PartnerCode,
            RequestId = Guid.NewGuid().ToString("N"),
            OrderId = request.PaymentId.ToString("N"),
            Amount = request.Amount,
            OrderInfo = $"Thanh toán giao dịch {request.PaymentId:D}",
            RedirectUrl = _hookBackOptions.RedirectUrl,
            IpnUrl = _momoOptions.IpnUrl
        };

        momoRequest.Signature = CreateSignature(momoRequest);
        return momoRequest;
    }

    private string CreateSignature(MomoCreatePaymentRequest request)
    {
        // The field order must match MoMo's create-payment signature specification.
        string rawData = $"accessKey={_momoOptions.AccessKey}"
            + $"&amount={request.Amount.ToString(CultureInfo.InvariantCulture)}"
            + $"&extraData={request.ExtraData}"
            + $"&ipnUrl={request.IpnUrl}"
            + $"&orderId={request.OrderId}"
            + $"&orderInfo={request.OrderInfo}"
            + $"&partnerCode={request.PartnerCode}"
            + $"&redirectUrl={request.RedirectUrl}"
            + $"&requestId={request.RequestId}"
            + $"&requestType={request.RequestType}";

        byte[] keyBytes = Encoding.UTF8.GetBytes(_momoOptions.SecretKey);
        byte[] dataBytes = Encoding.UTF8.GetBytes(rawData);
        byte[] hashBytes = HMACSHA256.HashData(keyBytes, dataBytes);

        return Convert.ToHexStringLower(hashBytes);
    }

    public async Task<PaymentCheckoutResult> CheckoutAsync(
        PaymentCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        MomoCreatePaymentRequest momoRequest = BuildCreatePaymentRequest(request);
        string? responseBody = null;
        int? statusCode = null;

        try
        {
            Console.WriteLine(momoRequest.IpnUrl);
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("create", momoRequest, cancellationToken);
            statusCode = (int)response.StatusCode;
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "MoMo HTTP error. PaymentId: {PaymentId}, OrderId: {OrderId}, RequestId: {RequestId}, StatusCode: {StatusCode}, ResponseBody: {ResponseBody}",
                    request.PaymentId, momoRequest.OrderId, momoRequest.RequestId, statusCode, responseBody);
                throw new Exception(ExceptionMessage.PaymentGatewayRequestFailed);
            }

            MomoCreatePaymentResponse? result = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(responseBody);
            if (result == null || result.ResultCode != 0 || string.IsNullOrWhiteSpace(result.PayUrl))
            {
                _logger.LogWarning(
                    "MoMo rejected create payment or returned an incomplete result. PaymentId: {PaymentId}, OrderId: {OrderId}, RequestId: {RequestId}, StatusCode: {StatusCode}, ResultCode: {ResultCode}, Message: {MomoMessage}, HasPayUrl: {HasPayUrl}, ResponseBody: {ResponseBody}",
                    request.PaymentId, momoRequest.OrderId, momoRequest.RequestId, statusCode,
                    result?.ResultCode, result?.Message, !string.IsNullOrWhiteSpace(result?.PayUrl), responseBody);
                throw new BusinessException(BusinessExceptionCode.CONFLICT, ExceptionMessage.PaymentGatewayRequestFailed);
            }

            return new PaymentCheckoutResult(result.PayUrl);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception,
                "MoMo connection error. PaymentId: {PaymentId}, OrderId: {OrderId}, RequestId: {RequestId}, StatusCode: {StatusCode}",
                request.PaymentId, momoRequest.OrderId, momoRequest.RequestId, statusCode);
            throw new BusinessException(BusinessExceptionCode.CONFLICT, ExceptionMessage.PaymentGatewayRequestFailed);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception,
                "MoMo returned invalid JSON. PaymentId: {PaymentId}, OrderId: {OrderId}, RequestId: {RequestId}, StatusCode: {StatusCode}, ResponseBody: {ResponseBody}",
                request.PaymentId, momoRequest.OrderId, momoRequest.RequestId, statusCode, responseBody);
            throw new BusinessException(BusinessExceptionCode.CONFLICT, ExceptionMessage.PaymentGatewayRequestFailed);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(exception,
                "MoMo request timed out. PaymentId: {PaymentId}, OrderId: {OrderId}, RequestId: {RequestId}, TimeoutSeconds: {TimeoutSeconds}",
                request.PaymentId, momoRequest.OrderId, momoRequest.RequestId, _httpClient.Timeout.TotalSeconds);
            throw new BusinessException(BusinessExceptionCode.CONFLICT, ExceptionMessage.PaymentGatewayRequestFailed);
        }
    }
}
