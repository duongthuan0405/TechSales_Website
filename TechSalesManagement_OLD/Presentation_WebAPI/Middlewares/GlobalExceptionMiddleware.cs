using System.Net;
using System.Text.Json;
using TechSalesManagement.Application.Exceptions;
using TechSalesManagement.Presentation_WebAPI.DTOs.Common;
using TechSalesManagement.Common;

namespace TechSalesManagement.Presentation_WebAPI.Middlewares;

public class GlobalExceptionMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = HttpStatusCode.InternalServerError;
        var message = ApiMessages.InternalServerError;
        Dictionary<string, List<string>>? errors = null;

        // Map custom business exceptions to HTTP status codes
        switch (exception)
        {
            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                errors = notFoundEx.Errors;
                break;

            case ConflictException conflictEx:
                statusCode = HttpStatusCode.Conflict;
                message = conflictEx.Message;
                errors = conflictEx.Errors;
                break;

            case BadRequestException badRequestEx:
                statusCode = HttpStatusCode.BadRequest;
                message = badRequestEx.Message;
                errors = badRequestEx.Errors;
                break;

            case UnauthorizedException unauthorizedEx:
                statusCode = HttpStatusCode.Unauthorized;
                message = unauthorizedEx.Message;
                errors = unauthorizedEx.Errors;
                break;

            case ForbiddenException forbiddenEx:
                statusCode = HttpStatusCode.Forbidden;
                message = forbiddenEx.Message;
                errors = forbiddenEx.Errors;
                break;

            case BusinessException businessEx:
                statusCode = HttpStatusCode.BadRequest;
                message = businessEx.Message;
                errors = businessEx.Errors;
                break;

            default:
                errors = new Dictionary<string, List<string>>
                {
                    { "server errors", new List<string> { exception.Message } }
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse(message, errors);
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var result = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(result);
    }
}
