namespace AuthModule.Application.UseCases.RefreshToken;
internal sealed record RefreshToken_UCOutput(string Token, DateTimeOffset ExpiresAt, string RefreshToken, DateTimeOffset RefreshExpiresAt);
