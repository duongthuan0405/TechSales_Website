using AuthModule.Domain.Constants;
using MediatR;
namespace AuthModule.Application.UseCases.GetStaff;
internal sealed record GetStaff_UCInput(Guid ActorId, int PageNumber = AuthLimits.DefaultPageNumber, int PageSize = AuthLimits.DefaultPageSize, string? Role = null) : IRequest<GetStaff_UCOutput>;
