using System.Diagnostics;
using Serilog.Context;

namespace TechSalesManagement.Presentation_WebAPI.Middlewares;

public class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var traceId = context.TraceIdentifier;
        
        using (LogContext.PushProperty("TraceId", traceId))
        {
            var sw = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = context.Request.Path;

            try
            {
                await next(context);
                sw.Stop();

                var statusCode = context.Response.StatusCode;
                
                // Log successful request
                _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms", 
                    method, path, statusCode, sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception)
            {
                sw.Stop();
                // We don't need to log the error here as GlobalExceptionMiddleware handles it
                _logger.LogWarning("HTTP {Method} {Path} failed in {Elapsed:0.0000} ms", 
                    method, path, sw.Elapsed.TotalMilliseconds);
                throw; 
            }
        }
    }
}
