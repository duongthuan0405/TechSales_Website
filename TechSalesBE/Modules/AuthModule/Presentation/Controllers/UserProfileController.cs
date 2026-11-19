using AuthModule.Application.UseCases.UnlockUser;
using AuthModule.Application.UseCases.UpdateProfile;
using AuthModule.Domain.Constants;
using AuthModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthModule.Presentation.Controllers;
[ApiController]
[Authorize]
[Route("api/user-profile")]
public sealed class UserProfileController : ControllerBase
{
    private readonly ISender _sender;

    public UserProfileController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AuthLimits.ProfileRequestMaxSizeBytes)]
    [ProducesResponseType(typeof(ApiSuccessResponse<UpdateProfile_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromForm] UpdateProfileRequestDto request, CancellationToken ct)
    {
        await using var avatar = request.AvatarFile?.OpenReadStream();
        UpdateProfile_UCOutput result = await _sender.Send(new UpdateProfile_UCInput(this.GetActorId(), request.FullName, request.Phone, request.AvatarUrl, request.DateOfBirth, avatar, request.AvatarFile?.ContentType, request.AvatarFile?.Length ?? 0), ct);
        ApiSuccessResponse<UpdateProfile_UCOutput> response = new ApiSuccessResponse<UpdateProfile_UCOutput>(null, result.Message);
        return Ok(response);
    }
}
