namespace AuthModule.Application.Services;

internal sealed class IssuedAccessToken
{
    public string Token { get; }
    public DateTimeOffset ExpiresAt { get; }

    public IssuedAccessToken(string token, DateTimeOffset expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }
}
