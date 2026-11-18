using MediatR;

namespace PromotionModule.Application.UseCases.GetVouchersForCustomer;

internal sealed record GetVouchersForCustomer_UCInput(int PageNumber, int PageSize) : IRequest<GetVouchersForCustomer_UCOutput>;
