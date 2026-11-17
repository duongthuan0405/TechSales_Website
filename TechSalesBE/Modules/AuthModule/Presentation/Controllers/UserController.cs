using AuthModule.Application.Messages;
using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using AuthModule.Application.UseCases.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        GetMe_UCOutput result = await _sender.Send(new GetMe_UCInput(this.GetActorId()), ct);
        ApiSuccessResponse<GetMe_UCOutput> response = new ApiSuccessResponse<GetMe_UCOutput>(result, ResponseMessage.SuccessAccount);
        return Ok(response);
    }
}
