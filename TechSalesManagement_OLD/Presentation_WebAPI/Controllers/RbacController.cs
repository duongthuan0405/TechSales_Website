using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Domain.Entities;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/admin/rbac")]
[Authorize(Roles = "Admin")]
public class RbacController : ControllerBase
{
    private readonly IRbacService _rbacService;

    public RbacController(IRbacService rbacService)
    {
        _rbacService = rbacService;
    }

    [HttpGet("roles")]
    public async Task<ActionResult<ApiSuccessResponse<List<Role>>>> GetRolesAsync()
    {
        var roles = await _rbacService.GetRolesAsync();
        return Ok(new ApiSuccessResponse<List<Role>>(roles, "Roles retrieved successfully."));
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<ApiSuccessResponse<List<Permission>>>> GetPermissionsAsync()
    {
        var permissions = await _rbacService.GetPermissionsAsync();
        return Ok(new ApiSuccessResponse<List<Permission>>(permissions, "Permissions retrieved successfully."));
    }

    [HttpPut("roles/{id}/permissions")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UpdateRolePermissionsAsync([FromRoute] Guid id, [FromBody] UpdatePermissionsRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _rbacService.UpdateRolePermissionsAsync(id, request.permissionIds, staffId.Value);
        return Ok(new ApiSuccessResponse<object>(null, "Role permissions updated successfully."));
    }

    [HttpPut("users/{userId}/roles")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> AssignUserRolesAsync([FromRoute] Guid userId, [FromBody] AssignRolesRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _rbacService.AssignUserRolesAsync(userId, request.roleIds, staffId.Value);
        return Ok(new ApiSuccessResponse<object>(null, "User roles assigned successfully."));
    }
}

public class UpdatePermissionsRequestDto
{
    public List<Guid> permissionIds { get; set; } = new();
}

public class AssignRolesRequestDto
{
    public List<Guid> roleIds { get; set; } = new();
}
