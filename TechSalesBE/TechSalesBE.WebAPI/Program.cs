using CommonRegistration = CommonModule.Presentation.ModuleRegistration.ModuleRegistration;
using AuthRegistration = AuthModule.Presentation.ModuleRegistration.ModuleRegistration;
using CatalogRegistration = CatalogModule.Presentation.ModuleRegistration.ModuleRegistration;
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
            CatalogRegistration.RegisterModule(builder.Services, builder.Configuration);
            InventoryRegistration.RegisterModule(builder.Services, builder.Configuration);
            CartRegistration.RegisterModule(builder.Services, builder.Configuration);
            OrderingRegistration.RegisterModule(builder.Services, builder.Configuration);
            ShippingRegistration.RegisterModule(builder.Services, builder.Configuration);
            PromotionRegistration.RegisterModule(builder.Services, builder.Configuration);
            AfterSaleRegistration.RegisterModule(builder.Services, builder.Configuration);
            PaymentRefundRegistration.RegisterModule(builder.Services, builder.Configuration);
            ReviewNotificationRegistration.RegisterModule(builder.Services, builder.Configuration);
            MailRegistration.RegisterModule(builder.Services, builder.Configuration);

            var app = builder.Build();
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi();
            }

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }

}
