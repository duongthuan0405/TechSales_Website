using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Common;
using TechSalesManagement.Application.HelperServices;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Application.Services.Params;
using TechSalesManagement.Domain.Entities;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.RequestDTOs;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;
using TechSalesManagement.Presentation_WebAPI.Extensions;

namespace TechSalesManagement.Presentation_WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthController(IAuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> RegisterAsync([FromBody] RegisterRequestDto request)
    {
        var parameters = new RegisterParams
        {
            Email = request.email,
            Password = request.password,
            ConfirmPassword = request.confirmPassword
        };
        
        await _authService.RegisterAsync(parameters);
        
        return Created(string.Empty, new ApiSuccessResponse<object>(null, MessageConstants.MSG6));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiSuccessResponse<LoginResponseDto>>> LoginAsync([FromBody] LoginRequestDto request)
    {
        var parameters = new LoginParams
        {
            Email = request.email,
            Password = request.password
        };
        
        var user = await _authService.LoginAsync(parameters);
        
        var token = _jwtService.GenerateToken(user, user.roles);
        
        var response = new LoginResponseDto
        {
            token = token,
            email = user.email,
            roles = user.roles.ConvertAll(r => r.name)
        };
        
        return Ok(new ApiSuccessResponse<LoginResponseDto>(response, MessageConstants.MSG11));
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> VerifyEmailAsync([FromBody] VerifyEmailRequestDto request)
    {
        var parameters = new VerifyEmailParams
        {
            Email = request.email,
            Token = request.token
        };
        
        await _authService.VerifyEmailAsync(parameters);
        
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG7));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> ForgotPasswordAsync([FromBody] ForgotPasswordRequestDto request)
    {
        var parameters = new ForgotPasswordParams
        {
            Email = request.email
        };
        
        await _authService.ForgotPasswordAsync(parameters);
        
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG13));
    }

    [HttpPut("reset-password")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> ResetPasswordAsync([FromBody] ResetPasswordRequestDto request)
    {
        var parameters = new ResetPasswordParams
        {
            Email = request.email,
            Token = request.token,
            NewPassword = request.newPassword,
            ConfirmPassword = request.confirmPassword
        };
        
        await _authService.ResetPasswordAsync(parameters);
        
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG14));
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> ChangePasswordAsync([FromBody] ChangePasswordRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();
        
        var parameters = new ChangePasswordParams
        {
            UserId = userId.Value,
            CurrentPassword = request.currentPassword,
            NewPassword = request.newPassword,
            ConfirmPassword = request.confirmPassword
        };
        
        await _authService.ChangePasswordAsync(parameters);
        
        return Ok(new ApiSuccessResponse<object>(null, MessageConstants.MSG20));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiSuccessResponse<object>>> LogoutAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        var token = authHeader.Replace("Bearer ", "").Trim();
        
        if (!string.IsNullOrEmpty(token))
        {
            await _authService.LogoutAsync(token);
        }

        return Ok(new ApiSuccessResponse<object>(null, "Logged out successfully. Token has been blacklisted."));
    }
}
