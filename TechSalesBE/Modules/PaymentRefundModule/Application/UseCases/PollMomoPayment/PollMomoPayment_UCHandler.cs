using System.Globalization;
using MediatR;
using PaymentRefundModule.Application.Repositories;
using PaymentRefundModule.Application.Services.Payments;
using PaymentRefundModule.Application.UseCases.ProcessPaymentNotification;
using PaymentRefundModule.Domain.Entities;
using PaymentRefundModule.Domain.Enums;
using PaymentRefundModule.Domain.Rules;

namespace PaymentRefundModule.Application.UseCases.PollMomoPayment;

internal sealed class PollMomoPayment_UCHandler : IRequestHandler<PollMomoPayment_UCInput, PollMomoPayment_UCOutput>
{
    private readonly IPaymentRepository _payments;
    private readonly IMomoStatusQuery _query;
    private readonly ISender _sender;

    public PollMomoPayment_UCHandler(IPaymentRepository payments, IMomoStatusQuery query, ISender sender)
    {
        _payments = payments;
        _query = query;
        _sender = sender;
    }

    public async Task<PollMomoPayment_UCOutput> Handle(PollMomoPayment_UCInput request, CancellationToken cancellationToken)
    {
        Payment? payment = await _payments.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment == null || !PaymentRules.IsMomoPollingAllowed(payment))
        {
            return new PollMomoPayment_UCOutput(false, null);
        }
        MomoStatusResult result = await _query.QueryAsync(payment.Id, cancellationToken);
        if (result.ResultCode != 0)
        {
            return new PollMomoPayment_UCOutput(false, result.ResultCode);
        }
        if (result.OrderId != payment.Id.ToString("N") || result.Amount != payment.Amount || result.TransId <= 0)
        {
            throw new InvalidOperationException("MoMo query result does not match the payment.");
        }

        ProcessPaymentNotification_UCOutput updated = await _sender.Send(
            new ProcessPaymentNotification_UCInput(payment.Id, result.Amount, "MOMO",
                result.TransId.ToString(CultureInfo.InvariantCulture), PaymentNotificationStatus.Success),
            cancellationToken);
        return new PollMomoPayment_UCOutput(updated.Updated, result.ResultCode);
    }
}

