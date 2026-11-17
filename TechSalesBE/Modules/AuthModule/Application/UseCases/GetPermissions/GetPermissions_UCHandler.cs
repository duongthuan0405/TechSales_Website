using AuthModule.Application.Constants;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Entities;
using MediatR;

namespace AuthModule.Application.UseCases.GetPermissions;
internal sealed class GetPermissions_UCHandler : IRequestHandler<GetPermissions_UCInput, GetPermissions_UCOutput>
{
    private readonly IPermissionRepository _permissions;

    public GetPermissions_UCHandler(IPermissionRepository permissions)
    {
        _permissions = permissions;
    }

    public async Task<GetPermissions_UCOutput> Handle(GetPermissions_UCInput input, CancellationToken ct)
    {
        List<Permission> entities = await _permissions.GetAllAsync(ct);
        List<GetPermissions_UCOutput.PermissionItem> permissionItems = new List<GetPermissions_UCOutput.PermissionItem>();
        foreach (Permission permission in entities)
        {
            GetPermissions_UCOutput.PermissionItem permissionItem = new GetPermissions_UCOutput.PermissionItem
            {
                Id = permission.Id,
                Code = permission.Code,
                Name = permission.Name,
                Module = permission.Module
            };
            permissionItems.Add(permissionItem);
        }

        return new GetPermissions_UCOutput
        {
            Permissions = permissionItems
        };
    }
}
