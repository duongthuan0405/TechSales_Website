using MediatR;

namespace PaymentRefundModule.Application.UseCases.ReceiveMomoIpn;

internal sealed record ReceiveMomoIpn_UCInput(
    string PartnerCode, string OrderId, string RequestId, long Amount,
    string OrderInfo, string OrderType, long TransId, int ResultCode,
    string Message, string PayType, long ResponseTime, string ExtraData,
    string Signature) : IRequest<ReceiveMomoIpn_UCOutput>;
