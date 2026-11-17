using AuthModule.Application.Exceptions;
using System.Security.Claims;
using AuthModule.Application.Repositories;
using AuthModule.Application.Services;
using AuthModule.Domain.Enums;
using AuthModule.Domain.Entities;
using CommonModule.BusinessExceptions;
using CommonModule.Presentation.ApiResponseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
namespace AuthModule.Infrastructure.AuthN;
internal static partial class ServiceCollectionExtensions
{
    internal static IServiceCollection ConfigAuthJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new JwtSettings(configuration);
        services.AddSingleton(settings);
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
                ValidateIssuerSigningKey = true, ValidIssuer = settings.Issuer, ValidAudience = settings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(settings.Key), ClockSkew = TimeSpan.Zero,
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256], NameClaimType = "email", RoleClaimType = "role"
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    ClaimsPrincipal? principal = context.Principal;
                    if (principal == null)
                    {
                        context.Fail(BusinessExceptionMessage.IdentityInvalid);
                        return;
                    }

                    string? subject = principal.FindFirstValue("sub");
                    Guid userId;
                    if (!Guid.TryParse(subject, out userId))
                    {
                        context.Fail(BusinessExceptionMessage.IdentityInvalid);
                        return;
                    }

                    IServiceProvider provider = context.HttpContext.RequestServices;
                    CancellationToken cancellationToken = context.HttpContext.RequestAborted;
                    IUserRepository users = provider.GetRequiredService<IUserRepository>();
                    User? user = await users.GetByIdAsync(userId, cancellationToken);
                    if (user == null || user.Status != UserStatus.ACTIVE)
                    {
                        context.Fail(BusinessExceptionMessage.AccountInactive);
                        return;
                    }

                    // Apply the current roles to an already-issued JWT.
                    ClaimsIdentity identity = (ClaimsIdentity)principal.Identity!;
                    foreach (Claim claim in identity.FindAll("role").ToArray())
                    {
                        identity.RemoveClaim(claim);
                    }
                    foreach (Role role in user.Roles)
                    {
                        identity.AddClaim(new Claim("role", role.Name));
                    }
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.WWWAuthenticate = "Bearer";
                    await context.Response.WriteAsJsonAsync(new ApiErrorResponse(BusinessExceptionMessage.AuthenticationRequired)
                        { Code = BusinessExceptionCode.UNAUTHORIZED }, context.HttpContext.RequestAborted);
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new ApiErrorResponse(BusinessExceptionMessage.AccessDenied)
                        { Code = BusinessExceptionCode.FORBIDDEN }, context.HttpContext.RequestAborted);
                }
            };
        });
        return services;
    }
}
