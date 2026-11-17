using MediatR;
namespace AuthModule.Application.UseCases.AssignUserRoles;
internal sealed record AssignUserRoles_UCInput(Guid ActorId, Guid UserId, Guid[] RoleIds) : IRequest<AssignUserRoles_UCOutput>;
