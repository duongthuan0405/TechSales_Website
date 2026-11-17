using AuthModule.Application.Constants;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using MediatR;

namespace AuthModule.Application.UseCases.GetRoles;
internal sealed class GetRoles_UCHandler : IRequestHandler<GetRoles_UCInput, GetRoles_UCOutput>
{
    private readonly IRoleRepository _roles;

    public GetRoles_UCHandler(IRoleRepository roles)
    {
        _roles = roles;
    }

    public async Task<GetRoles_UCOutput> Handle(GetRoles_UCInput input, CancellationToken ct)
    {
        List<Role> entities = await _roles.GetAllAsync(ct);
        List<GetRoles_UCOutput.RoleItem> roleItems = new List<GetRoles_UCOutput.RoleItem>();
        foreach (Role role in entities)
        {
            List<GetRoles_UCOutput.PermissionItem> permissionItems = new List<GetRoles_UCOutput.PermissionItem>();
            foreach (Permission permission in role.Permissions)
            {
                GetRoles_UCOutput.PermissionItem permissionItem = new GetRoles_UCOutput.PermissionItem
                {
                    Id = permission.Id,
                    Code = permission.Code,
                    Name = permission.Name,
                    Module = permission.Module
                };
                permissionItems.Add(permissionItem);
            }

            GetRoles_UCOutput.RoleItem roleItem = new GetRoles_UCOutput.RoleItem
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Permissions = permissionItems
            };
            roleItems.Add(roleItem);
        }

        return new GetRoles_UCOutput
        {
            Roles = roleItems
        };
    }
}
