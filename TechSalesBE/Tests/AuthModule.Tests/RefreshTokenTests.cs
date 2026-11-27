using System.Net;
using System.Net.Http.Json;
using AuthModule.Application.Constants;
using AuthModule.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AuthModule.Tests.AuthTestApp;

namespace AuthModule.Tests;

public sealed class RefreshTokenTests
{
    [Fact]
    public async Task Credentialed_cors_allows_only_the_configured_frontend()
    {
        await using var app = await AuthTestApp.CreateAsync(new() { ["AUTH:FrontendUrl"] = "https://shop.example.com" });
        foreach (var origin in new[] { "https://shop.example.com", "https://other.example.net" })
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/refresh");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "POST");
            request.Headers.Add("Access-Control-Request-Headers", "X-Requested-With");
            var response = await app.Client.SendAsync(request);
            if (origin == "https://shop.example.com")
            {
                Assert.Equal(origin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
                Assert.Equal("true", response.Headers.GetValues("Access-Control-Allow-Credentials").Single());
            }
            else Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }

    private static string Cookie(HttpResponseMessage response) => response.Headers.GetValues("Set-Cookie")
        .Single(value => value.StartsWith("techsales_refresh=")).Split(';')[0];

    private static async Task<(string Access, string Refresh)> Login(AuthTestApp app, string email)
    {
        var response = await app.Client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
        var json = await ExpectAsync(response, HttpStatusCode.OK);
        Assert.False(json.GetProperty("data").TryGetProperty("refreshToken", out _));
        var header = response.Headers.GetValues("Set-Cookie").Single();
        Assert.Contains("httponly", header.ToLowerInvariant());
        Assert.Contains("samesite=strict", header.ToLowerInvariant());
        Assert.Contains("path=/api/auth", header);
        Assert.Contains("secure", header.ToLowerInvariant());
        return (json.GetProperty("data").GetProperty("token").GetString()!, Cookie(response));
    }

    private static Task<HttpResponseMessage> Send(AuthTestApp app, string cookie, string endpoint = "refresh")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/" + endpoint);
        if (!string.IsNullOrEmpty(cookie)) request.Headers.Add("Cookie", cookie);
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        return app.Client.SendAsync(request);
    }

    [Fact]
    public async Task Refresh_rotates_cookie_stores_only_hash_and_revokes_family_on_reuse()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("refresh@example.com", UserRoleName.CUSTOMER);
        var login = await Login(app, "refresh@example.com");
        var row = await app.DbAsync(db => db.Set<RefreshSessionDbModel>().SingleAsync());
        Assert.Equal(64, row.TokenHash.Length);
        Assert.DoesNotContain(row.TokenHash, login.Refresh);
        var refreshed = await Send(app, login.Refresh);
        var json = await ExpectAsync(refreshed, HttpStatusCode.OK);
        Assert.False(json.GetProperty("data").TryGetProperty("refreshToken", out _));
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("data").GetProperty("token").GetString()));
        var next = Cookie(refreshed);
        Assert.NotEqual(login.Refresh, next);
        Assert.Equal(2, await app.DbAsync(db => db.Set<RefreshSessionDbModel>().CountAsync()));
        await ExpectAsync(await Send(app, login.Refresh), HttpStatusCode.Unauthorized);
        await ExpectAsync(await Send(app, next), HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_revokes_cookie_without_requiring_a_valid_access_token()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("logout-refresh@example.com", UserRoleName.CUSTOMER);
        var login = await Login(app, "logout-refresh@example.com");
        var logout = await Send(app, login.Refresh, "logout");
        await ExpectAsync(logout, HttpStatusCode.OK);
        Assert.Contains("techsales_refresh=;", logout.Headers.GetValues("Set-Cookie").Single());
        await ExpectAsync(await Send(app, login.Refresh), HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Missing_expired_or_cross_site_refresh_is_rejected()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("expired-refresh@example.com", UserRoleName.CUSTOMER);
        var login = await Login(app, "expired-refresh@example.com");
        var crossSite = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        crossSite.Headers.Add("Cookie", login.Refresh);
        await ExpectAsync(await app.Client.SendAsync(crossSite), HttpStatusCode.BadRequest);
        await ExpectAsync(await Send(app, ""), HttpStatusCode.Unauthorized);
        await app.DbAsync(async db =>
        {
            var session = await db.Set<RefreshSessionDbModel>().SingleAsync();
            session.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            return await db.SaveChangesAsync();
        });
        await ExpectAsync(await Send(app, login.Refresh), HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("change-password")]
    [InlineData("reset-password")]
    [InlineData("lock")]
    [InlineData("deactivate")]
    public async Task Password_and_account_state_changes_revoke_refresh_sessions(string action)
    {
        await using var app = await AuthTestApp.CreateAsync();
        var userId = await app.SeedUserAsync("state-refresh@example.com", UserRoleName.CUSTOMER);
        var login = await Login(app, "state-refresh@example.com");
        if (action == "change-password")
            await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put, "/api/auth/change-password", login.Access,
                new { currentPassword = Password, newPassword = "Different!Password123", confirmPassword = "Different!Password123" })), HttpStatusCode.OK);
        else if (action == "reset-password")
        {
            await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "state-refresh@example.com" }), HttpStatusCode.OK);
            await ExpectAsync(await app.Client.PutAsJsonAsync("/api/auth/reset-password", new { email = "state-refresh@example.com",
                token = app.Mail.Reset["state-refresh@example.com"], newPassword = "Different!Password123", confirmPassword = "Different!Password123" }), HttpStatusCode.OK);
        }
        else
        {
            await app.SeedUserAsync("state-admin@example.com", UserRoleName.TECHNICAL_ADMIN);
            var admin = await app.LoginAsync("state-admin@example.com");
            await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, $"/api/admin/users/{userId}/{action}", admin,
                new { until = (DateTimeOffset?)null })), HttpStatusCode.OK);
        }
        await ExpectAsync(await Send(app, login.Refresh), HttpStatusCode.Unauthorized);
        Assert.False(await app.DbAsync(db => db.Set<RefreshSessionDbModel>().AnyAsync(s => s.UserId == userId && s.RevokedAt == null)));
    }
}
