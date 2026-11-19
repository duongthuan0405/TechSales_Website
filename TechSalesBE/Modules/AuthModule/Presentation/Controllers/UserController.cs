using AuthModule.Application.Messages;
using AuthModule.Application.UseCases.GetMe;
using AuthModule.Application.UseCases.UnlockUser;
using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthModule.Presentation.Controllers;
[ApiController]
[Authorize]
[Route("api/user")]
public sealed class UserController : ControllerBase
{
    private readonly ISender _sender;

    public UserController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetMe_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        GetMe_UCOutput result = await _sender.Send(new GetMe_UCInput(this.GetActorId()), ct);
        ApiSuccessResponse<GetMe_UCOutput> response = new ApiSuccessResponse<GetMe_UCOutput>(result, ResponseMessage.SuccessAccount);
        return Ok(response);
    }
}
