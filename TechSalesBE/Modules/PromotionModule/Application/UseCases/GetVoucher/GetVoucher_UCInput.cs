using MediatR;

namespace PromotionModule.Application.UseCases.GetVoucher;

internal sealed record GetVoucherForCustomer_UCInput(Guid Id) : IRequest<GetVoucherForCustomer_UCOutput>;
