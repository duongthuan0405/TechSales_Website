using CommonModule.Application.Exceptions;
using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using CommonModule.Presentation.ApiResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CommonModule.Presentation.Middlewares;

public sealed class GlobalExceptionMiddleware : IMiddleware
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(exception,
                    "Cannot write an error response because the response has started. TraceId: {TraceId}",
                    context.TraceIdentifier);
                throw;
            }

            var statusCode = StatusCodes.Status500InternalServerError;
            var response = new ApiErrorResponse(BusinessExceptionMessage.UnexpectedError)
            {
                Code = BusinessExceptionCode.INTERNAL_SERVER_ERROR
            };

            if (exception is BusinessException businessException)
            {
                statusCode = businessException.Code switch
                {
                    BusinessExceptionCode.BAD_REQUEST => StatusCodes.Status400BadRequest,
                    BusinessExceptionCode.UNAUTHORIZED => StatusCodes.Status401Unauthorized,
                    BusinessExceptionCode.FORBIDDEN => StatusCodes.Status403Forbidden,
                    BusinessExceptionCode.NOT_FOUND => StatusCodes.Status404NotFound,
                    BusinessExceptionCode.CONFLICT => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };

                if (statusCode != StatusCodes.Status500InternalServerError)
                {
                    response = new ApiErrorResponse(businessException.Message, businessException.Errors)
                    {
                        Code = businessException.Code
                    };
                }
            }
            else if (exception is DomainException domainException)
            {
                statusCode = StatusCodes.Status422UnprocessableEntity;
                response = new ApiErrorResponse(domainException.Message)
                {
                    Code = domainException.Code
                };
            }

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            }
            else
            {
                _logger.LogWarning("Request failed with code {Code} and status {StatusCode}. TraceId: {TraceId}",
                    response.Code, statusCode, context.TraceIdentifier);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(response, cancellationToken: context.RequestAborted);
        }
    }
}
