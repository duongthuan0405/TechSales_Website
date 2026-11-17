using System.Globalization;
using CommonModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using CommonModule.Presentation.ApiResponseModels;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CommonModule.Presentation.Configuration;

public static class UserResponseConfiguration
{
    public static IServiceCollection AddVietnameseResponses(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("vi");
        services.PostConfigure<ApiBehaviorOptions>(options =>
        {
            foreach (KeyValuePair<int, ClientErrorData> mapping in options.ClientErrorMapping)
            {
                if (mapping.Key >= StatusCodes.Status500InternalServerError)
                {
                    mapping.Value.Title = BusinessExceptionMessage.UnexpectedError;
                }
                else
                {
                    mapping.Value.Title = BusinessExceptionMessage.InvalidRequest;
                }
            }

            options.InvalidModelStateResponseFactory = context =>
            {
                Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
                foreach (string key in context.ModelState.Keys)
                {
                    if (context.ModelState[key]!.Errors.Count > 0)
                    {
                        errors.Add(key, new List<string> { BusinessExceptionMessage.InvalidRequest });
                    }
                }

                ApiErrorResponse response = new ApiErrorResponse(BusinessExceptionMessage.InvalidRequest, errors)
                {
                    Code = BusinessExceptionCode.BAD_REQUEST
                };
                return new BadRequestObjectResult(response);
            };
        });
        return services;
    }
}
