namespace TechSalesManagement.Application.Common.Configurations;

public class MailSettingsCO
{
    public string host { get; set; } = string.Empty;
    public int port { get; set; }
    public string userName { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string fromEmail { get; set; } = string.Empty;
    public string displayName { get; set; } = string.Empty;
    public bool enableSsl { get; set; } = true;
}
