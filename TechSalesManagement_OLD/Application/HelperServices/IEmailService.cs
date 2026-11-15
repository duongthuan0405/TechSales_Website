using System.Threading.Tasks;

namespace TechSalesManagement.Application.HelperServices;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string to, string verificationLink);
    Task SendPasswordResetEmailAsync(string to, string resetLink);
    Task SendOrderConfirmationEmailAsync(string to, System.Guid orderId, decimal totalAmount, string shippingAddress);
}
