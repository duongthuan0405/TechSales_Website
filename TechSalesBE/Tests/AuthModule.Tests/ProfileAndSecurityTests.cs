using AuthModule.Domain.Constants;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthModule.Application.Constants;
using AuthModule.Domain.Entities;
using AuthModule.Domain.Enums;
using AuthModule.Infrastructure.Persistence;
using AuthModule.Infrastructure.Services;
using CommonModule.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using static AuthModule.Tests.AuthTestApp;

namespace AuthModule.Tests;
public sealed class ProfileAndSecurityTests
{
    [Fact]
    public async Task Profile_updates_only_the_authenticated_user_and_uploads_an_avatar()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var token = await app.LoginAsync("customer@example.com");
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Example Customer"), "fullName");
        form.Add(new StringContent("0901234567"), "phone");
        form.Add(new StringContent("2000-01-01"), "dateOfBirth");
        var png = new ByteArrayContent(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a0WQAAAAASUVORK5CYII="));
        png.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(png, "avatarFile", "ignored-client-filename.png");
        using var request = app.Authorized(HttpMethod.Put, "/api/user-profile", token);
        request.Content = form;
        await ExpectAsync(await app.Client.SendAsync(request), HttpStatusCode.OK);
        var me = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", token)), HttpStatusCode.OK);
        var profile = me.GetProperty("data").GetProperty("profile");
        Assert.Equal("Example Customer", profile.GetProperty("fullName").GetString());
        Assert.Equal("0901234567", profile.GetProperty("phone").GetString());
        Assert.StartsWith("/avatars/", profile.GetProperty("avatarUrl").GetString());
    }

    [Fact]
    public async Task Invalid_profile_and_spoofed_image_do_not_update_persisted_profile()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var token = await app.LoginAsync("customer@example.com");
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Example Customer"), "fullName");
        form.Add(new StringContent("0901234567"), "phone");
        var fake = new ByteArrayContent("<script>alert(1)</script>"u8.ToArray());
        fake.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(fake, "avatarFile", "fake.png");
        using var request = app.Authorized(HttpMethod.Put, "/api/user-profile", token);
        request.Content = form;
        await ExpectAsync(await app.Client.SendAsync(request), HttpStatusCode.BadRequest);
        var profile = await app.DbAsync(db => db.UserProfiles.SingleAsync());
        Assert.Equal("", profile.FullName);
        Assert.Null(profile.AvatarUrl);
    }

    [Fact]
    public async Task Public_auth_endpoints_limit_repeated_attempts()
    {
        const int configuredRequestLimit = 3;
        Dictionary<string, string?> configuration = new Dictionary<string, string?>
        {
            ["AUTH:RateLimit:PermitLimit"] = configuredRequestLimit.ToString(),
            ["AUTH:RateLimit:WindowMinutes"] = "1"
        };
        await using AuthTestApp app = await AuthTestApp.CreateAsync(configuration);
        for (int i = 0; i < configuredRequestLimit; i++)
        {
            await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/verify-email", new { email = "invalid", token = "x" }), HttpStatusCode.BadRequest);
        }

        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/verify-email", new { email = "invalid", token = "x" }), HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Forged_jwt_is_rejected()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var token = await app.LoginAsync("customer@example.com");
        token = token[..token.LastIndexOf('.')] + ".AAAA";
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", token)), HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void Domain_expired_lock_is_released_but_manual_lock_is_preserved()
    {
        var now = DateTimeOffset.UtcNow;
        var user = User.Create("customer@example.com", "stored-hash", verified: true);
        user.LockAccount(now.AddMinutes(1), now);
        user.ReleaseExpiredLock(now.AddMinutes(2));
        Assert.Equal(UserStatus.ACTIVE, user.Status);
        user.LockAccount(null, now.AddMinutes(2));
        user.ChangePassword("new-hash", now.AddDays(1));
        Assert.Equal(UserStatus.BLOCKED, user.Status);
        Assert.Null(user.LockedUntil);
        Assert.Throws<DomainException>(() => User.Create("bad-email", "stored-hash"));
    }

    [Fact]
    public void Otp_is_six_digits_and_password_hash_is_salted()
    {
        var otp = new OtpService().GenerateOtp(OtpType.EMAIL_VERIFICATION, Guid.NewGuid());
        Assert.Matches(AuthModule.Domain.Rules.AuthRules.OtpPattern, otp.Otp);
        Assert.InRange(otp.ExpiredAt, DateTimeOffset.UtcNow.AddMinutes(AuthLimits.OtpLifetimeMinutes - 1), DateTimeOffset.UtcNow.AddMinutes(AuthLimits.OtpLifetimeMinutes + 1));
        var hasher = new PasswordHasher();
        var first = hasher.HashPassword(Password);
        Assert.NotEqual(first, hasher.HashPassword(Password));
        Assert.True(hasher.VerifyPassword(Password, first));
        Assert.False(hasher.VerifyPassword("wrong", first));
        Assert.Throws<DomainException>(() => hasher.HashPassword(new string ('a', AuthLimits.PasswordMaxUtf8Bytes + 1)));
    }

    [Fact]
    public void PostgreSql_model_matches_snapshot_and_new_migration_only_inserts_seed_data()
    {
        using var db = new AuthDbContext(new DbContextOptionsBuilder<AuthDbContext>().UseNpgsql("Host=localhost;Database=not_used;Username=not_used;Password=not_used", npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", AuthDbContext.Schema)).Options);
        Assert.False(db.Database.HasPendingModelChanges());
        var migrations = db.Database.GetMigrations().ToArray();
        string seedMigration = migrations.Single(migration => migration.EndsWith("_SeedAuthRolesAndPermissions", StringComparison.Ordinal));
        string sql = db.GetService<IMigrator>().GenerateScript(migrations[0], seedMigration, MigrationsSqlGenerationOptions.Idempotent);
        Assert.Contains("INSERT INTO identify_access.", sql);
        Assert.DoesNotContain("CREATE TABLE inventory", sql);
        Assert.DoesNotContain("DROP TABLE", sql);
        Assert.Contains("Technical Admin", sql);
    }

    [Fact]
    public void Profile_updates_do_not_extend_the_failed_login_window()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        User user = User.Create("customer@example.com", "stored-hash", verified: true);
        user.FailedLoginAttempts = 2;
        user.LastLoginFailedAt = now.AddMinutes(-AuthLimits.FailedLoginWindowMinutes);
        user.UpdatedAt = now;
        user.RecordFailedLogin(now);
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Equal(now, user.LastLoginFailedAt);
    }

    [Fact]
    public void Recent_failure_is_counted_even_if_updated_at_is_old()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        User user = User.Create("customer@example.com", "stored-hash", verified: true);
        user.FailedLoginAttempts = 1;
        user.LastLoginFailedAt = now.AddMinutes(-1);
        user.UpdatedAt = now.AddMinutes(-AuthLimits.FailedLoginWindowMinutes);
        user.RecordFailedLogin(now);
        Assert.Equal(2, user.FailedLoginAttempts);
        Assert.Equal(now, user.LastLoginFailedAt);
    }

    [Fact]
    public async Task Failed_login_timestamp_is_persisted_and_successful_login_clears_it()
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        Guid userId = await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        HttpResponseMessage failedResponse = await app.Client.PostAsJsonAsync("/api/auth/login", new { email = "customer@example.com", password = "wrong-password" });
        await ExpectAsync(failedResponse, HttpStatusCode.Unauthorized);
        DateTimeOffset? failedAt = await app.DbAsync(db => db.Users.Where(user => user.Id == userId).Select(user => user.LastLoginFailedAt).SingleAsync());
        Assert.NotNull(failedAt);
        await app.LoginAsync("customer@example.com");
        DateTimeOffset? clearedFailedAt = await app.DbAsync(db => db.Users.Where(user => user.Id == userId).Select(user => user.LastLoginFailedAt).SingleAsync());
        Assert.Null(clearedFailedAt);
        Assert.Equal(0, await app.DbAsync(db => db.Users.Where(user => user.Id == userId).Select(user => user.FailedLoginAttempts).SingleAsync()));
    }
}
