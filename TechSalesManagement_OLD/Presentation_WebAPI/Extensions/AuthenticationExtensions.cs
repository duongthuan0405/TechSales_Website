using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TechSalesManagement.Application.Services.Interfaces;

namespace TechSalesManagement.Presentation_WebAPI.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JWT:secretKey"] ?? throw new InvalidOperationException("JWT Secret Key not found in configuration.");
        var issuer = configuration["JWT:issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration.");
        var audience = configuration["JWT:audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration.");

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set true in production if needed
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    var token = authHeader.Replace("Bearer ", "").Trim();
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        var isBlacklisted = await cacheService.GetAsync<bool>($"blacklist:token:{token}");
                        if (isBlacklisted)
                        {
                            context.Fail("This token is no longer valid (logged out).");
                        }
                    }
                }
            };
        });

        return services;
    }
}
