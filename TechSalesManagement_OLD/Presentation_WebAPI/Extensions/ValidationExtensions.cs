using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Presentation_WebAPI.DTOs.ResponseDTOs;

namespace TechSalesManagement.Presentation_WebAPI.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidationConfiguration(this IServiceCollection services)
    {
        // 1. Register FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        // 2. Override default 400 behavior to return ApiErrorResponse
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                var response = new ApiErrorResponse("Validation failed", errors);
                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}
