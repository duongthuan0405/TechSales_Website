namespace PaymentRefundModule.Application.UseCases.ReceiveVnpayIpn;

internal sealed record ReceiveVnpayIpn_UCOutput(string RspCode, string Message);
