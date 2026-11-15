
using AuthModule.Presentation.ModuleRegistration;
using CommonModule.Presentation.ModuleRegistration;
using CommonModule.Presentation.Middlewares;

namespace TechSalesBE.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //builder.Services.RegisterAuthModule(builder.Configuration);
            builder.Services.RegisterCommonModule(builder.Configuration);

            var app = builder.Build();
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }

}
