using TechSalesManagement.Presentation_WebAPI.Extensions;
using TechSalesManagement.Infrastructure.Persistence;

namespace TechSalesManagement;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load environment variables from .env.development
        DotNetEnv.Env.Load(".env.development");

        var builder = WebApplication.CreateBuilder(args);

        // Enable Serilog Logging
        builder.Host.UseSerilogConfiguration();

        // Add services to the container.
        builder.Services.AddAuthenticationConfiguration(builder.Configuration);
        builder.Services.AddAuthorization();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

        // Configure Swagger via Extension Method
        builder.Services.AddSwaggerConfiguration();
        
        // Configure Middleware services
        builder.Services.AddMiddlewares();
        // Configure Validation
        builder.Services.AddValidationConfiguration();
        // Configure Options
        builder.Services.AddConfigurationOptions(builder.Configuration);
        // Configure Repositories
        builder.Services.AddRepositories();
        // Configure External and Helper Services
        builder.Services.AddExternalAndHelperServices(builder.Configuration);
        // Configure Application Services
        builder.Services.AddApplicationServices();

        // Register Persistence (Database)
        builder.Services.AddPersistence();

        var app = builder.Build();

        // Configure Middleware Pipeline
        app.UseMiddlewares(app.Environment);

        app.MapControllers();

        await app.RunAsync();
    }
}
