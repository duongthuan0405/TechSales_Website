using MediatR;

namespace PromotionModule.Application.UseCases.GetVouchers;

internal sealed record GetVouchers_UCInput(int PageNumber, int PageSize) : IRequest<GetVouchers_UCOutput>;
