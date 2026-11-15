using System;
using System.Text.RegularExpressions;

namespace TechSalesManagement.Application.Common.Utils;

public static class ValidationUtils
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^[0-9]{10,11}$", RegexOptions.Compiled);

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex.IsMatch(email);
    }

    public static bool IsValidPhoneNumber(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        return PhoneRegex.IsMatch(phone);
    }

    public static bool IsValidPasswordFormat(string? password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return password.Length >= 8;
    }
}
