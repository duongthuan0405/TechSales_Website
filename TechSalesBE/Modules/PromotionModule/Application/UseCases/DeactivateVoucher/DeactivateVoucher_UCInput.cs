using MediatR;

namespace PromotionModule.Application.UseCases.DeactivateVoucher;

internal sealed record DeactivateVoucher_UCInput(Guid Id) : IRequest<DeactivateVoucher_UCOutput>;
