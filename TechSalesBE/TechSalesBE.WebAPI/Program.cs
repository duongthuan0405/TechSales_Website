using CommonRegistration = CommonModule.Presentation.ModuleRegistration.ModuleRegistration;
using AuthRegistration = AuthModule.Presentation.ModuleRegistration.ModuleRegistration;
using ProductRegistration = ProductModule.Presentation.ModuleRegistration.ModuleRegistration;
using InventoryRegistration = InventoryModule.Presentation.ModuleRegistration.ModuleRegistration;
using CartRegistration = CartModule.Presentation.ModuleRegistration.ModuleRegistration;
using OrderingRegistration = OrderingModule.Presentation.ModuleRegistration.ModuleRegistration;
using ShippingRegistration = ShippingModule.Presentation.ModuleRegistration.ModuleRegistration;
using PromotionRegistration = PromotionModule.Presentation.ModuleRegistration.ModuleRegistration;
using AfterSaleRegistration = AfterSaleModule.Presentation.ModuleRegistration.ModuleRegistration;
using PaymentRefundRegistration = PaymentRefundModule.Presentation.ModuleRegistration.ModuleRegistration;
using ReviewNotificationRegistration = ReviewNotificationModule.Presentation.ModuleRegistration.ModuleRegistration;
using MailRegistration = MailModule.Presentation.ModuleRegistration.ModuleRegistration;
using CommonModule.Presentation.Middlewares;
using TechSalesBE.WebAPI.Integration;
using Microsoft.Extensions.FileProviders;

namespace TechSalesBE.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Environment.IsDevelopment())
            {
                DotNetEnv.Env.NoClobber().Load();
                builder.Configuration.AddEnvironmentVariables();
                builder.Configuration.AddCommandLine(args);
            }

            // Add services to the container.

            builder.Services.AddOpenApiDocument(settings =>
            {
                settings.Title = "TechSales API";
                settings.Version = "v1";
                settings.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
                {
                    Type = NSwag.OpenApiSecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT access token."
                });
                settings.OperationProcessors.Add(
                    new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("Bearer"));
            });

            CommonRegistration.RegisterModule(builder.Services, builder.Configuration);
            AuthRegistration.RegisterModule(builder.Services, builder.Configuration);
            ProductRegistration.RegisterModule(builder.Services, builder.Configuration);
            InventoryRegistration.RegisterModule(builder.Services, builder.Configuration);
            CartRegistration.RegisterModule(builder.Services, builder.Configuration);
            OrderingRegistration.RegisterModule(builder.Services, builder.Configuration);
            ShippingRegistration.RegisterModule(builder.Services, builder.Configuration);
            PromotionRegistration.RegisterModule(builder.Services, builder.Configuration);
            AfterSaleRegistration.RegisterModule(builder.Services, builder.Configuration);
            PaymentRefundRegistration.RegisterModule(builder.Services, builder.Configuration);
            ReviewNotificationRegistration.RegisterModule(builder.Services, builder.Configuration);
            MailRegistration.RegisterModule(builder.Services, builder.Configuration);
            IntegrationRegistration.RegisterModule(builder.Services);

            var app = builder.Build();
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi();
            }

            //app.UseHttpsRedirection();

            var webRoot = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(webRoot);
            app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(webRoot) });
            app.UseRouting();
            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }

}
