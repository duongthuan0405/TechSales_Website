using AuthModule.Domain.Constants;
using MediatR;
namespace AuthModule.Application.UseCases.GetStaff;
internal sealed record GetStaff_UCInput(Guid ActorId, int PageNumber = AuthLimits.DefaultPageNumber, int PageSize = AuthLimits.DefaultPageSize) : IRequest<GetStaff_UCOutput>;
