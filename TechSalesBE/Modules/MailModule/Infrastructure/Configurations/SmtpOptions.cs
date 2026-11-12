namespace MailModule.Infrastructure.Configurations;

public enum SmtpSecurity
{
    StartTls,
    SslOnConnect,
    None
}

public sealed class SmtpOptions
{
    public const string SectionName = "MAIL";
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string FromEmail { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public SmtpSecurity Security { get; set; } = SmtpSecurity.StartTls;
    public int TimeoutMilliseconds { get; set; } = 30000;
}
