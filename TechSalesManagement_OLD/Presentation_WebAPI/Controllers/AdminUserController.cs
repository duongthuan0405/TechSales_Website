using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Common;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Staff,Business Admin,Technical Admin")]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUserController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("customers")]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<User>>>> GetCustomersAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var (items, totalCount) = await _adminUserService.GetCustomersAsync(pageNumber, pageSize);
        var response = new PagedResponseDto<User>
        {
            items = items,
            totalCount = totalCount,
            pageNumber = pageNumber,
            pageSize = pageSize
        };
        return Ok(new ApiSuccessResponse<PagedResponseDto<User>>(response, "Customer list retrieved successfully."));
    }

    [HttpGet("staff")]
    public async Task<ActionResult<ApiSuccessResponse<PagedResponseDto<User>>>> GetStaffAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var requesterId = User.GetUserId();
        if (requesterId == null) return Unauthorized();

        var (items, totalCount) = await _adminUserService.GetStaffAsync(pageNumber, pageSize, requesterId.Value);
        var response = new PagedResponseDto<User>
        {
            items = items,
            totalCount = totalCount,
            pageNumber = pageNumber,
            pageSize = pageSize
        };
        return Ok(new ApiSuccessResponse<PagedResponseDto<User>>(response, "Staff list retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiSuccessResponse<User>>> CreateUserAsync([FromBody] CreateUserRequestDto request)
    {
        var requesterId = User.GetUserId();
        if (requesterId == null) return Unauthorized();

        var user = new User
        {
            email = request.email,
            roles = request.roles.Select(r => new Role { name = r }).ToList()
        };

        var created = await _adminUserService.CreateStaffAsync(user, request.password, requesterId.Value);
        return Ok(new ApiSuccessResponse<User>(created, "User created successfully."));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiSuccessResponse<User>>> UpdateUserAsync([FromRoute] Guid id, [FromBody] User user)
    {
        var updated = await _adminUserService.UpdateStaffAsync(id, user);
        return Ok(new ApiSuccessResponse<User>(updated, "User updated successfully."));
    }

    [HttpPost("{id}/lock")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> LockCustomerAsync([FromRoute] Guid id, [FromBody] LockUserRequestDto request)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _adminUserService.LockCustomerAsync(id, request.until, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, "User locked successfully."));
    }

    [HttpPost("{id}/unlock")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UnlockCustomerAsync([FromRoute] Guid id)
    {
        var staffId = User.GetUserId();
        if (staffId == null) return Unauthorized();

        await _adminUserService.UnlockCustomerAsync(id, staffId.Value);

        return Ok(new ApiSuccessResponse<object>(null, "User unlocked successfully."));
    }
}

public class CreateUserRequestDto
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public List<string> roles { get; set; } = new();
}

public class LockUserRequestDto
{
    public DateTimeOffset? until { get; set; }
}
