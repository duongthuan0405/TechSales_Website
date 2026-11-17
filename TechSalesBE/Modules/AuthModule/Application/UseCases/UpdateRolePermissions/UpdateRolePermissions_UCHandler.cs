using AuthModule.Application.Constants;
using AuthModule.Application.Ports;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Rules;
using CommonModule.BusinessExceptions;
using MediatR;

namespace AuthModule.Application.UseCases.UpdateRolePermissions;
internal sealed class UpdateRolePermissions_UCHandler : IRequestHandler<UpdateRolePermissions_UCInput, UpdateRolePermissions_UCOutput>
{
    private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IPermissionRepository _permissions;
    private readonly IExecuteAtomically _transaction;

    public UpdateRolePermissions_UCHandler(IUserRepository users, IRoleRepository roles, IPermissionRepository permissions, IExecuteAtomically transaction)
    {
        _users = users;
        _roles = roles;
        _permissions = permissions;
        _transaction = transaction;
    }

    public async Task<UpdateRolePermissions_UCOutput> Handle(UpdateRolePermissions_UCInput input, CancellationToken ct)
    {
        return await _transaction.ExecuteAtomicallyAsync(async () =>
        {
            await _users.LockUsersAsync([input.ActorId], ct);
            await _roles.LockAsync(input.RoleId, ct);
            var role = (await _roles.GetByIdsAsync([input.RoleId], ct)).SingleOrDefault() ?? throw new BusinessException(BusinessExceptionCode.NOT_FOUND, "Role not found.");
            if (role.Name == UserRoleName.TECHNICAL_ADMIN)
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "Technical Admin permissions cannot be modified.");
            }

            if (!await _permissions.AllExistAsync(input.PermissionIds, ct))
            {
                throw new BusinessException(BusinessExceptionCode.BAD_REQUEST, "Unknown permission.");
            }

            await _permissions.ReplaceRolePermissionsAsync(input.RoleId, input.PermissionIds, ct);
            return new UpdateRolePermissions_UCOutput("Role permissions updated successfully.");
        }, ct);
    }
}
