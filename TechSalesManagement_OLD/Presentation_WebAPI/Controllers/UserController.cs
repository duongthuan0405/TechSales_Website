using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiSuccessResponse<UserResponseDto>>> GetMeAsync()
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        
        var parameters = new GetUserByIdParams
        {
            UserId = userId.Value
        };
        
        var user = await _userService.GetByIdAsync(parameters);
        if (user == null) return NotFound(new ApiSuccessResponse<object>(null, MessageConstants.MSG117));

        var response = new UserResponseDto
        {
            id = user.id,
            email = user.email,
            status = user.status,
            createdAt = user.createdAt,
            roles = user.roles.Select(r => r.name).ToList(),
            profile = user.profile == null ? null : new ProfileResponseDto
            {
                fullName = user.profile.fullName,
                phone = user.profile.phone,
                avatarUrl = user.profile.avatarUrl,
                dateOfBirth = user.profile.dateOfBirth
            }
        };

        return Ok(new ApiSuccessResponse<UserResponseDto>(response, MessageConstants.MSG118));
    }
}
