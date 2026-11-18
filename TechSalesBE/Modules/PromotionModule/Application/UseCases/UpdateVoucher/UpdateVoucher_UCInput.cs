using MediatR;

namespace PromotionModule.Application.UseCases.UpdateVoucher;

internal sealed record UpdateVoucher_UCInput(Guid Id, string Code, string Type, long Value, int? MaxUsage, long? MinOrderAmount, DateTimeOffset? StartDate, DateTimeOffset? EndDate) : IRequest<UpdateVoucher_UCOutput>;
