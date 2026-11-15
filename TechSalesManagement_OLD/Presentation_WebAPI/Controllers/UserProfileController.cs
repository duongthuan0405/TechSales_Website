using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Common;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/user-profile")]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<ApiSuccessResponse<object>>> UpdateProfileAsync([FromForm] UpdateProfileRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var parameters = new UpdateProfileParams
        {
            UserId = userId.Value,
            FullName = request.fullName,
            Phone = request.phone,
            AvatarUrl = request.avatarUrl,
            AvatarFile = request.avatarFile,
            DateOfBirth = request.dateOfBirth
        };

        await _userProfileService.UpdateProfileAsync(parameters);

        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG17));
    }
}
