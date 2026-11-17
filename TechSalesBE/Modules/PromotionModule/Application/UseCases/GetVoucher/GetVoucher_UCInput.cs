using MediatR;

namespace PromotionModule.Application.UseCases.GetVoucher;

internal sealed record GetVoucher_UCInput(Guid Id) : IRequest<GetVoucher_UCOutput>;
