namespace AuthModule.Infrastructure.Configurations;

internal sealed class AuthRateLimitOptions
{
    public const string SectionName = "AUTH:RateLimit";

    public int PermitLimit { get; set; } = 20;
    public int WindowMinutes { get; set; } = 1;
}
