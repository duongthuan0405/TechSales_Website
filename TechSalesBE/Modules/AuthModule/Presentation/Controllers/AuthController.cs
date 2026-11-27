using AuthModule.Application.Messages;
using AuthModule.Application.UseCases.ChangePassword;
using AuthModule.Application.UseCases.ForgotPassword;
using AuthModule.Application.UseCases.Login;
using AuthModule.Application.UseCases.Logout;
using AuthModule.Application.UseCases.Register;
using AuthModule.Application.UseCases.ResetPassword;
using AuthModule.Application.UseCases.UnlockUser;
using AuthModule.Application.UseCases.VerifyEmail;
using AuthModule.Application.UseCases.RefreshToken;
using AuthModule.Presentation.DTOs;
using CommonModule.Presentation.ApiResponseModels;
using CommonModule.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AuthModule.Presentation.Controllers;
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookie = "techsales_refresh";
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
    [ProducesResponseType(typeof(ApiSuccessResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken ct)
    {
        Login_UCOutput result = await _sender.Send(new Login_UCInput(request.Email, request.Password), ct);
        SetRefreshCookie(result.RefreshToken, result.RefreshExpiresAt);
        Response.Headers.CacheControl = "no-store";
        var response = new ApiSuccessResponse<LoginResponseDto>(new LoginResponseDto(result.Token, result.Email, result.Roles, result.ExpiresAt), ResponseMessage.SuccessLogin);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [EnableRateLimiting("auth-public")]
    [ProducesResponseType(typeof(ApiSuccessResponse<RefreshTokenResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        // A custom header prevents cross-site form submissions; CORS controls allowed origins.
        if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
            return BadRequest(new ApiErrorResponse("Yêu cầu làm mới phiên không hợp lệ."));
        try
        {
            var result = await _sender.Send(new RefreshToken_UCInput(Request.Cookies[RefreshCookie] ?? ""), ct);
            SetRefreshCookie(result.RefreshToken, result.RefreshExpiresAt);
            Response.Headers.CacheControl = "no-store";
            return Ok(new ApiSuccessResponse<RefreshTokenResponseDto>(new(result.Token, result.ExpiresAt)));
        }
        catch (CommonModule.BusinessExceptions.BusinessException exception)
            when (exception.Code == CommonModule.BusinessExceptions.BusinessExceptionCode.UNAUTHORIZED)
        {
            Response.Cookies.Delete(RefreshCookie, RefreshCookieOptions());
            throw;
        }
    }

    private CookieOptions RefreshCookieOptions() => new()
    {
        HttpOnly = true, Secure = Request.IsHttps || !HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment(),
        SameSite = SameSiteMode.Strict, Path = "/api/auth",
    };

    private void SetRefreshCookie(string token, DateTimeOffset expires)
    {
        var options = RefreshCookieOptions();
        options.Expires = expires;
        Response.Cookies.Append(RefreshCookie, token, options);
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

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiSuccessResponse<Logout_UCOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.ContainsKey(RefreshCookie) && Request.Headers["X-Requested-With"] != "XMLHttpRequest")
            return BadRequest(new ApiErrorResponse("Yêu cầu đăng xuất không hợp lệ."));
        Guid.TryParse(User.FindFirst("sub")?.Value, out var actorId);
        Logout_UCOutput result = await _sender.Send(new Logout_UCInput(actorId, Request.Cookies[RefreshCookie] ?? ""), ct);
        Response.Cookies.Delete(RefreshCookie, RefreshCookieOptions());
        ApiSuccessResponse<Logout_UCOutput> response = new ApiSuccessResponse<Logout_UCOutput>(null, result.Message);
        return Ok(response);
    }
}
