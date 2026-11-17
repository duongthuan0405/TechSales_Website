using MediatR;
namespace AuthModule.Application.UseCases.GetRoles;
internal sealed record GetRoles_UCInput(Guid ActorId) : IRequest<GetRoles_UCOutput>;
