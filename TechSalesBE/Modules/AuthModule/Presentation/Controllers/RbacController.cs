using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using AuthModule.Application.Constants;
using AuthModule.Application.UseCases.GetRoles;
using AuthModule.Application.UseCases.GetPermissions;
using AuthModule.Application.UseCases.UpdateRolePermissions;
using AuthModule.Application.UseCases.AssignUserRoles;
using AuthModule.Presentation.DTOs;
using CommonModule.CustomAttributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthModule.Presentation.Controllers;
[ApiController]
[Authorize(Roles = UserRoleName.TECHNICAL_ADMIN)]
[HasPermission(AuthPermission.RBAC_MANAGE)]
[Route("api/admin/rbac")]
public sealed class RbacController : ControllerBase
{
    private readonly ISender _sender;

    public RbacController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        GetRoles_UCOutput result = await _sender.Send(new GetRoles_UCInput(this.GetActorId()), ct);
        ApiSuccessResponse<List<GetRoles_UCOutput.RoleItem>> response = new ApiSuccessResponse<List<GetRoles_UCOutput.RoleItem>>(result.Roles, "Roles retrieved successfully.");
        return Ok(response);
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
    {
        GetPermissions_UCOutput result = await _sender.Send(new GetPermissions_UCInput(this.GetActorId()), ct);
        ApiSuccessResponse<List<GetPermissions_UCOutput.PermissionItem>> response = new ApiSuccessResponse<List<GetPermissions_UCOutput.PermissionItem>>(result.Permissions, "Permissions retrieved successfully.");
        return Ok(response);
    }

    [HttpPut("roles/{id:guid}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(Guid id, UpdatePermissionsRequestDto request, CancellationToken ct)
    {
        UpdateRolePermissions_UCOutput result = await _sender.Send(new UpdateRolePermissions_UCInput(this.GetActorId(), id, request.PermissionIds), ct);
        ApiSuccessResponse<UpdateRolePermissions_UCOutput> response = new ApiSuccessResponse<UpdateRolePermissions_UCOutput>(null, result.Message);
        return Ok(response);
    }

    [HttpPut("users/{userId:guid}/roles")]
    public async Task<IActionResult> AssignUserRoles(Guid userId, AssignRolesRequestDto request, CancellationToken ct)
    {
        AssignUserRoles_UCOutput result = await _sender.Send(new AssignUserRoles_UCInput(this.GetActorId(), userId, request.RoleIds), ct);
        ApiSuccessResponse<AssignUserRoles_UCOutput> response = new ApiSuccessResponse<AssignUserRoles_UCOutput>(null, result.Message);
        return Ok(response);
    }
}
