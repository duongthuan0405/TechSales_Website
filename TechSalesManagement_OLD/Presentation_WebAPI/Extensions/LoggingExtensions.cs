using Serilog;

namespace TechSalesManagement.Presentation_WebAPI.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder UseSerilogConfiguration(this IHostBuilder host)
    {
        return host.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{TraceId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB limit per file
                    retainedFileCountLimit: 31,           // Keep logs for 31 days
                    rollOnFileSizeLimit: true,            // Create new file when size limit is reached
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{TraceId}] {Message:lj}{NewLine}{Exception}");
        });
    }
}
