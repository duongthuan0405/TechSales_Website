namespace TechSalesManagement.Application.Common.Configurations;

public class JwtCO
{
    public string secretKey { get; set; } = string.Empty;
    public string issuer { get; set; } = string.Empty;
    public string audience { get; set; } = string.Empty;
    public int durationInMinutes { get; set; } = 1440; // 1 day default
}
