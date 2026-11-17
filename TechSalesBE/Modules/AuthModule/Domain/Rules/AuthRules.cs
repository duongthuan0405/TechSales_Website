using AuthModule.Domain.Constants;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
namespace AuthModule.Domain.Rules;
internal static class AuthRules
{
    public static string PasswordRequirementMessage => $"Password must have at least {AuthLimits.PasswordMinLength} characters and at most {AuthLimits.PasswordMaxUtf8Bytes} UTF-8 bytes.";
    public static string PhoneRequirementMessage => $"Phone must have {AuthLimits.PhoneMinDigits} to {AuthLimits.PhoneMaxDigits} digits.";
    public static string OtpPattern => $@"^[0-9]{{{AuthLimits.OtpLength}}}$";
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    public static bool ValidEmail(string? email) => !string.IsNullOrWhiteSpace(email)
        && email.Length <= AuthLimits.EmailMaxLength && MailAddress.TryCreate(email.Trim(), out var address)
        && address.Address == email.Trim() && address.Host.Contains('.');
    // BCrypt's input limit is measured in UTF-8 bytes, not characters.
    public static bool ValidPassword(string? password) => !string.IsNullOrWhiteSpace(password)
        && password.Length >= AuthLimits.PasswordMinLength && Encoding.UTF8.GetByteCount(password) <= AuthLimits.PasswordMaxUtf8Bytes;
    public static bool ValidPhone(string? phone) => phone is not null
        && Regex.IsMatch(phone, $@"^[0-9]{{{AuthLimits.PhoneMinDigits},{AuthLimits.PhoneMaxDigits}}}$", RegexOptions.None, TimeSpan.FromMilliseconds(100));
    public static bool ValidFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return false;
        }

        return fullName.Length <= AuthLimits.FullNameMaxLength;
    }

    public static bool ValidAvatarUrl(string? url)
    {
        if (url == null)
        {
            return true;
        }

        if (url.Length > AuthLimits.AvatarUrlMaxLength)
        {
            return false;
        }

        if (url.StartsWith("/avatars/", StringComparison.Ordinal))
        {
            return true;
        }

        Uri? parsedUrl;
        bool isAbsoluteUrl = Uri.TryCreate(url, UriKind.Absolute, out parsedUrl);
        if (!isAbsoluteUrl || parsedUrl == null)
        {
            return false;
        }

        return parsedUrl.Scheme == Uri.UriSchemeHttps;
    }

    public static bool ValidDateOfBirth(DateOnly? dateOfBirth, DateOnly today)
    {
        if (!dateOfBirth.HasValue)
        {
            return true;
        }

        return dateOfBirth.Value <= today;
    }
}
