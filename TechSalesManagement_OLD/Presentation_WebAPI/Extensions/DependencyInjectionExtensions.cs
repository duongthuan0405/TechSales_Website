using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechSalesManagement.Application.Common.Configurations;
using TechSalesManagement.Application.HelperServices;
using TechSalesManagement.Application.Interfaces;
using TechSalesManagement.Application.Services.Implementations;
using TechSalesManagement.Application.Services.Interfaces;
using TechSalesManagement.Infrastructure.HelperServices;
using TechSalesManagement.Application.Repositories;
using TechSalesManagement.Infrastructure.Repositories;
using TechSalesManagement.Infrastructure.Services;
using TechSalesManagement.Application.Services.Strategies.VoucherStrategies;
using TechSalesManagement.Application.Services.Strategies.Refund;
using TechSalesManagement.Application.Services.Strategies.PaymentStrategies;


namespace TechSalesManagement.Presentation_WebAPI.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OtpCO>(configuration.GetSection("OTP"));
        services.Configure<MailSettingsCO>(configuration.GetSection("MAIL"));
        services.Configure<FrontendCO>(configuration.GetSection("FE"));
        services.Configure<JwtCO>(configuration.GetSection("JWT"));
        services.Configure<MomoCO>(configuration.GetSection("Momo"));
        services.Configure<CloudinaryCO>(configuration.GetSection("Cloudinary"));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IShippingAddressService, ShippingAddressService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductManagementService, ProductManagementService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IVoucherManagementService, VoucherManagementService>();
        services.AddScoped<IContentManagementService, ContentManagementService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IRbacService, RbacService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IOrderManagementService, OrderManagementService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();

        // Discount Strategies
        services.AddScoped<IDiscountStrategy, FixedDiscountStrategy>();
        services.AddScoped<IDiscountStrategy, PercentDiscountStrategy>();
        services.AddScoped<IDiscountStrategyFactory, DiscountStrategyFactory>();

        // Refund Strategies
        services.AddScoped<IRefundStrategy, CodRefundStrategy>();
        services.AddScoped<IRefundStrategy, VnPayRefundStrategy>();
        services.AddScoped<IRefundStrategyFactory, RefundStrategyFactory>();

        // Payment Strategies
        services.AddScoped<IPaymentStrategy, CodPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, MomoPaymentStrategy>();
        services.AddScoped<IPaymentStrategy, VnPayPaymentStrategy>();
        services.AddScoped<IPaymentStrategyFactory, PaymentStrategyFactory>();

        return services;
    }

    public static IServiceCollection AddExternalAndHelperServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddHttpClient();

        // Configure Redis Cloud
        var redisConnectionString = configuration["Redis:ConnectionString"] ?? configuration["Redis__ConnectionString"] 
            ?? "localhost:6379";
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => 
            StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IShippingAddressRepository, ShippingAddressRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IVoucherRepository, VoucherRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IStatisticsRepository, StatisticsRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        return services;
    }
}
