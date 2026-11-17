using MediatR;
namespace AuthModule.Application.UseCases.UpdateRolePermissions;
internal sealed record UpdateRolePermissions_UCInput(Guid ActorId, Guid RoleId, Guid[] PermissionIds) : IRequest<UpdateRolePermissions_UCOutput>;
