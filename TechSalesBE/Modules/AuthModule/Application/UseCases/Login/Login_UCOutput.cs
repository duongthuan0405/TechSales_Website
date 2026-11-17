namespace AuthModule.Application.UseCases.Login;
internal sealed record Login_UCOutput(string Token, string Email, List<string> Roles, DateTimeOffset ExpiresAt);
