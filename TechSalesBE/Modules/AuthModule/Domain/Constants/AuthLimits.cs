namespace AuthModule.Domain.Constants;

/// <summary>Single source for Auth business limits and their default values.</summary>
public static class AuthLimits
{
    public const int EmailMaxLength = 255;
    public const int PasswordMinLength = 8;
    // UTF-8 byte limit required by the current password hashing algorithm.
    public const int PasswordMaxUtf8Bytes = 72;
    public const int PasswordInputMaxLength = 256;
    public const int FullNameMaxLength = 150;
    public const int PhoneMinDigits = 10;
    public const int PhoneMaxDigits = 11;
    public const int AvatarUrlMaxLength = 500;

    public const int MaxFailedLoginAttempts = 5;
    public const int FailedLoginWindowMinutes = 30;
    public const int AccountLockMinutes = 30;
    public const int OtpLength = 6;
    public const int OtpLifetimeMinutes = 5;

    // JWT configuration may override the default within the allowed range.
    public const int DefaultTokenLifetimeMinutes = 60;
    public const int MinTokenLifetimeMinutes = 1;
    public const int MaxTokenLifetimeMinutes = 1440;

    public const int MinPageNumber = 1;
    public const int MinPageSize = 1;
    public const int DefaultPageNumber = MinPageNumber;
    public const int DefaultPageSize = 20;
    public const int MaxPageNumber = 1_000_000;
    public const int MaxPageSize = 100;
    public const int MaxRolesPerUser = 4;
    public const int MaxPermissionsPerRole = 1000;
    public const int AvatarMaxSizeMegabytes = 5;
    public const int AvatarMaxSizeBytes = AvatarMaxSizeMegabytes * 1024 * 1024;
    public const int ProfileRequestMaxSizeBytes = AvatarMaxSizeBytes + 1024 * 1024;
}
