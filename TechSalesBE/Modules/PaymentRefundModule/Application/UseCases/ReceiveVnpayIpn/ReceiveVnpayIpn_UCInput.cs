using MediatR;

namespace PaymentRefundModule.Application.UseCases.ReceiveVnpayIpn;

internal sealed record ReceiveVnpayIpn_UCInput(Dictionary<string, string> Parameters) : IRequest<ReceiveVnpayIpn_UCOutput>;
