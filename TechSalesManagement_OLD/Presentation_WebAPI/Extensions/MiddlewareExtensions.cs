using TechSalesManagement.Presentation_WebAPI.Middlewares;

namespace TechSalesManagement.Presentation_WebAPI.Extensions;

public static class MiddlewareExtensions
{
    public static IServiceCollection AddMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<GlobalExceptionMiddleware>();
        services.AddTransient<RequestLoggingMiddleware>();

        // CORS: Allow FE (Vite dev server) to call BE
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseMiddlewares(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 1. CORS — MUST be first to handle preflight OPTIONS requests
        //    before any other middleware can reject them with 405
        app.UseCors();

        // 2. Exception Handling to catch all subsequent errors
        app.UseMiddleware<GlobalExceptionMiddleware>();

        // 3. Request Logging to monitor traffic and performance
        app.UseMiddleware<RequestLoggingMiddleware>();

        // 4. Swagger (Dev environment only)
        app.UseSwaggerConfiguration(env);

        // 5. Security & Auth
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
