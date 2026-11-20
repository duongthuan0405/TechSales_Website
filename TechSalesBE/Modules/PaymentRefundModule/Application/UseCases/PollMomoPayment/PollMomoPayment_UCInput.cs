using MediatR;
namespace PaymentRefundModule.Application.UseCases.PollMomoPayment;
internal sealed record PollMomoPayment_UCInput(Guid PaymentId) : IRequest<PollMomoPayment_UCOutput>;

