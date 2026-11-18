using MediatR;

namespace PromotionModule.Application.UseCases.CreateVoucher;

internal sealed record CreateVoucher_UCInput(string Code, string Type, long Value, int? MaxUsage, long? MinOrderAmount, DateTimeOffset? StartDate, DateTimeOffset? EndDate) : IRequest<CreateVoucher_UCOutput>;
