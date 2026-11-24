using AuthModule.Application.Messages;
using AuthModule.Application.UseCases.ChangePassword;
using AuthModule.Application.UseCases.ForgotPassword;
using AuthModule.Application.UseCases.Login;
using AuthModule.Application.UseCases.Logout;
using AuthModule.Application.UseCases.Register;
using AuthModule.Application.UseCases.ResetPassword;
using AuthModule.Application.UseCases.UnlockUser;
using AuthModule.Application.UseCases.VerifyEmail;
using AuthModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthModule.Presentation.Controllers;
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [EnableRateLimiting("auth-public")]
    [ProducesResponseType(typeof(ApiSuccessResponse<Register_UCOutput>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequestDto request, CancellationToken ct)
    {
        Register_UCOutput result = await _sender.Send(new Register_UCInput(request.Email, request.Password, request.ConfirmPassword, request.FullName, request.Phone), ct);
        ApiSuccessResponse<Register_UCOutput> response = new ApiSuccessResponse<Register_UCOutput>(null, result.Message);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("auth-public")]
    [ProducesResponseType(typeof(ApiSuccessResponse<Login_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken ct)
    {
        Login_UCOutput result = await _sender.Send(new Login_UCInput(request.Email, request.Password), ct);
        ApiSuccessResponse<Login_UCOutput> response = new ApiSuccessResponse<Login_UCOutput>(result, ResponseMessage.SuccessLogin);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    [EnableRateLimiting("auth-public")]
    [ProducesResponseType(typeof(ApiSuccessResponse<VerifyEmail_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequestDto request, CancellationToken ct)
    {
        VerifyEmail_UCOutput result = await _sender.Send(new VerifyEmail_UCInput(request.Email, request.Token), ct);
        ApiSuccessResponse<VerifyEmail_UCOutput> response = new ApiSuccessResponse<VerifyEmail_UCOutput>(null, result.Message);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth-public")]
    [ProducesResponseType(typeof(ApiSuccessResponse<ForgotPassword_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request, CancellationToken ct)
    {
        ForgotPassword_UCOutput result = await _sender.Send(new ForgotPassword_UCInput(request.Email), ct);
        ApiSuccessResponse<ForgotPassword_UCOutput> response = new ApiSuccessResponse<ForgotPassword_UCOutput>(null, result.Message);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPut("reset-password")]
    [EnableRateLimiting("auth-public")]
    [ProducesResponseType(typeof(ApiSuccessResponse<ResetPassword_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request, CancellationToken ct)
    {
        ResetPassword_UCOutput result = await _sender.Send(new ResetPassword_UCInput(request.Email, request.Token, request.NewPassword, request.ConfirmPassword), ct);
        ApiSuccessResponse<ResetPassword_UCOutput> response = new ApiSuccessResponse<ResetPassword_UCOutput>(null, result.Message);
        return Ok(response);
    }

    [Authorize]
    [HttpPut("change-password")]
    [ProducesResponseType(typeof(ApiSuccessResponse<ChangePassword_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request, CancellationToken ct)
    {
        ChangePassword_UCOutput result = await _sender.Send(new ChangePassword_UCInput(this.GetActorId(), request.CurrentPassword, request.NewPassword, request.ConfirmPassword), ct);
        ApiSuccessResponse<ChangePassword_UCOutput> response = new ApiSuccessResponse<ChangePassword_UCOutput>(null, result.Message);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiSuccessResponse<Logout_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        Logout_UCOutput result = await _sender.Send(new Logout_UCInput(this.GetActorId()), ct);
        ApiSuccessResponse<Logout_UCOutput> response = new ApiSuccessResponse<Logout_UCOutput>(null, result.Message);
        return Ok(response);
    }
}
