using AuthModule.Domain.Constants;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthModule.Application.Constants;
using AuthModule.Domain.Enums;
using AuthModule.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AuthModule.Tests.AuthTestApp;
namespace AuthModule.Tests;
public sealed class AuthFlowsTests
{
    [Fact]
    public async Task Register_verify_login_and_read_profile_without_leaking_secrets()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/register", new
            { email = " New@Example.com ", password = Password, confirmPassword = Password }), HttpStatusCode.Created);
        var row = await app.DbAsync(db => db.Users.Include(u => u.Profile).Include(u => u.UserRoles).ThenInclude(ur => ur.Role).SingleAsync());
        Assert.Equal("new@example.com", row.Email);
        Assert.Equal(UserStatus.PENDING, row.Status);
        Assert.NotNull(row.Profile);
        Assert.Equal(UserRoleName.CUSTOMER, Assert.Single(row.UserRoles).Role.Name);
        Assert.NotEqual(Password, row.HashedPassword);
        Assert.True(new PasswordHasher().VerifyPassword(Password, row.HashedPassword));
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/login", new { email = row.Email, password = Password }), HttpStatusCode.Unauthorized);
        var code = app.Mail.Verification[row.Email];
        var stored = await app.DbAsync(db => db.UserOtps.SingleAsync());
        Assert.NotEqual(code, stored.Value);
        string wrongCode = new string('0', AuthLimits.OtpLength);
        if (wrongCode == code)
        {
            wrongCode = new string('1', AuthLimits.OtpLength);
        }

        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/verify-email",
            new { email = row.Email, token = wrongCode }), HttpStatusCode.BadRequest);
        Assert.Equal(UserStatus.PENDING, await app.DbAsync(db => db.Users.Select(user => user.Status).SingleAsync()));
        Assert.Null(await app.DbAsync(db => db.UserOtps.Select(otp => otp.UsedAt).SingleAsync()));

        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/verify-email", new { email = row.Email, token = code }), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/verify-email", new { email = row.Email, token = code }), HttpStatusCode.BadRequest);
        var token = await app.LoginAsync(row.Email);
        var response = await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", token));
        var json = await ExpectAsync(response, HttpStatusCode.OK);
        Assert.Equal("ACTIVE", json.GetProperty("data").GetProperty("status").GetString());
        Assert.DoesNotContain("hashedPassword", json.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(row.HashedPassword, json.ToString());
    }
    [Fact]
    public async Task Validation_runs_before_writes_and_invalid_credentials_do_not_create_otp()
    {
        await using var app = await AuthTestApp.CreateAsync();
        var response = await app.Client.PostAsJsonAsync("/api/auth/register", new { email = "bad", password = "123", confirmPassword = "321" });
        var json = await ExpectAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal("BAD_REQUEST", json.GetProperty("code").GetString());
        Assert.Equal(0, await app.DbAsync(db => db.Users.CountAsync()));
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/login", new { email = "absent@example.com", password = Password }), HttpStatusCode.Unauthorized);
        Assert.Equal(0, await app.DbAsync(db => db.UserOtps.CountAsync()));
    }
    [Fact]
    public async Task Failed_attempts_commit_and_fifth_failure_blocks_authenticated_requests()
    {
        await using var app = await AuthTestApp.CreateAsync();
        var id = await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var token = await app.LoginAsync("customer@example.com");
        for (var attempt = 1; attempt <= AuthLimits.MaxFailedLoginAttempts; attempt++)
        {
            await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/login", new { email = "customer@example.com", password = "wrong" }),
                attempt == AuthLimits.MaxFailedLoginAttempts ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized);
            Assert.Equal(attempt, await app.DbAsync(db => db.Users.Where(u => u.Id == id).Select(u => u.FailedLoginAttempts).SingleAsync()));
        }
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", token)), HttpStatusCode.Unauthorized);
        Assert.Equal(UserStatus.BLOCKED, await app.DbAsync(db => db.Users.Where(u => u.Id == id).Select(u => u.Status).SingleAsync()));
    }
    [Fact]
    public async Task Logout_is_client_side_and_new_login_keeps_existing_jwt_valid()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var first = await app.LoginAsync("customer@example.com");
        var second = await app.LoginAsync("customer@example.com");
        System.IdentityModel.Tokens.Jwt.JwtSecurityToken decoded = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(first);
        Assert.DoesNotContain(decoded.Claims, claim => claim.Type == "jti");
        Assert.Equal(0, await app.DbAsync(db => db.UserOtps.CountAsync()));
        Assert.False(await app.DbAsync(db => Task.FromResult(db.Model.GetEntityTypes().Any(entity => entity.GetTableName() == "UserSession"))));
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", first)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/auth/logout", second)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", second)), HttpStatusCode.OK);
    }
    [Fact]
    public async Task Reset_code_is_single_use_and_password_changes_keep_existing_jwt_valid()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var first = await app.LoginAsync("customer@example.com");
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "customer@example.com" }), HttpStatusCode.OK);
        var reset = new { email = "customer@example.com", token = app.Mail.Reset["customer@example.com"], newPassword = "New!Password123", confirmPassword = "New!Password123" };
        await ExpectAsync(await app.Client.PutAsJsonAsync("/api/auth/reset-password", reset), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.PutAsJsonAsync("/api/auth/reset-password", reset), HttpStatusCode.BadRequest);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", first)), HttpStatusCode.OK);
        var next = await app.LoginAsync("customer@example.com", reset.newPassword);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put, "/api/auth/change-password", next,
            new { currentPassword = reset.newPassword, newPassword = Password, confirmPassword = Password })), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", next)), HttpStatusCode.OK);
        await app.LoginAsync("customer@example.com");
    }
    [Fact]
    public async Task Forgot_password_does_not_disclose_unknown_accounts()
    {
        await using var app = await AuthTestApp.CreateAsync();
        var response = await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "absent@example.com" }), HttpStatusCode.OK);
        Assert.Contains("If the account exists", response.GetProperty("message").GetString());
        Assert.Empty(app.Mail.Reset);
    }
    [Fact]
    public async Task Email_failure_rolls_back_registration()
    {
        await using var app = await AuthTestApp.CreateAsync();
        app.Mail.Fail = true;
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/register", new { email = "customer@example.com", password = Password, confirmPassword = Password }), HttpStatusCode.InternalServerError);
        Assert.Equal(0, await app.DbAsync(db => db.Users.CountAsync()));
        Assert.Equal(0, await app.DbAsync(db => db.UserOtps.CountAsync()));
    }
    [Fact]
    public async Task Expired_verification_code_does_not_activate_account()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/register", new { email = "customer@example.com", password = Password, confirmPassword = Password }), HttpStatusCode.Created);
        await app.DbAsync(async db => { (await db.UserOtps.SingleAsync()).ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(-1); return await db.SaveChangesAsync(); });
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/verify-email", new { email = "customer@example.com", token = app.Mail.Verification["customer@example.com"] }), HttpStatusCode.BadRequest);
        Assert.Equal(UserStatus.PENDING, await app.DbAsync(db => db.Users.Select(u => u.Status).SingleAsync()));
    }
}
