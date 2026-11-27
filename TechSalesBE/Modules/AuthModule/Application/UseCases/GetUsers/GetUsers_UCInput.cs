using AuthModule.Domain.Constants;
using MediatR;
namespace AuthModule.Application.UseCases.GetUsers;
internal sealed record GetUsers_UCInput(Guid ActorId, int PageNumber = AuthLimits.DefaultPageNumber, int PageSize = AuthLimits.DefaultPageSize, string? Role = null, string? Keyword = null) : IRequest<GetUsers_UCOutput>;
