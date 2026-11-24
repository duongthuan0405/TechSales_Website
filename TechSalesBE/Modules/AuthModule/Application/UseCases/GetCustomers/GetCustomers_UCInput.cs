using AuthModule.Domain.Constants;
using MediatR;
namespace AuthModule.Application.UseCases.GetCustomers;
internal sealed record GetCustomers_UCInput(Guid ActorId, int PageNumber = AuthLimits.DefaultPageNumber, int PageSize = AuthLimits.DefaultPageSize, string? Keyword = null) : IRequest<GetCustomers_UCOutput>;
