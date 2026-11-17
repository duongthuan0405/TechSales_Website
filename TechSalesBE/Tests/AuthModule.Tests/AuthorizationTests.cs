using System.Net;
using System.Net.Http.Json;
using AuthModule.Application.Constants;
using AuthModule.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AuthModule.Tests.AuthTestApp;

namespace AuthModule.Tests;
public sealed class AuthorizationTests
{
    [Fact]
    public async Task Seeds_have_exactly_four_roles_and_expected_permissions()
    {
        await using var app = await AuthTestApp.CreateAsync();
        var roles = await app.DbAsync(db => db.Roles.Select(r => r.Name).ToArrayAsync());
        Assert.Equal(new[] { "Business Admin", "Customer", "Staff", "Technical Admin" }, roles.Order().ToArray());
        Assert.Equal(5, await app.DbAsync(db => db.Permissions.CountAsync()));
        Assert.Equal(11, await app.DbAsync(db => db.RolePermissions.CountAsync()));
        await ExpectAsync(await app.Client.GetAsync("/api/user/me"), HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Customer_cannot_access_customer_lists_or_rbac()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var token = await app.LoginAsync("customer@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/customers", token)), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/rbac/roles", token)), HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Staff_can_lock_customers_but_cannot_lock_administrators_or_create_accounts()
    {
        await using var app = await AuthTestApp.CreateAsync();
        var customer = await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var admin = await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        await app.SeedUserAsync("staff@example.com", UserRoleName.STAFF);
        var token = await app.LoginAsync("staff@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/customers", token)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, $"/api/admin/users/{customer}/lock", token, new { until = (string? )null })), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, $"/api/admin/users/{admin}/lock", token, new { until = (string? )null })), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = "illegal@example.com", password = Password, roles = new[] { UserRoleName.TECHNICAL_ADMIN } })), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, $"/api/admin/users/{customer}/unlock", token)), HttpStatusCode.OK);
    }

    [Fact]
    public async Task Business_admin_can_create_staff_and_business_admin_but_not_technical_admin()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        var token = await app.LoginAsync("business@example.com");
        foreach (var role in new[]
        {
            UserRoleName.STAFF,
            UserRoleName.BUSINESS_ADMIN
        }

        )
        {
            await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = role.Replace(" ", "") + "@example.com", password = Password, roles = new[] { role } })), HttpStatusCode.Created);
        }

        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = "illegal@example.com", password = Password, roles = new[] { UserRoleName.TECHNICAL_ADMIN } })), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/staff", token)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/rbac/permissions", token)), HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Technical_admin_can_assign_roles_and_existing_jwt_uses_current_roles()
    {
        await using var app = await AuthTestApp.CreateAsync();
        var tech = await app.SeedUserAsync("tech@example.com", UserRoleName.TECHNICAL_ADMIN);
        var customer = await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var customerToken = await app.LoginAsync("customer@example.com");
        var token = await app.LoginAsync("tech@example.com");
        var roles = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/rbac/roles", token)), HttpStatusCode.OK);
        Assert.Equal(4, roles.GetProperty("data").GetArrayLength());
        foreach (System.Text.Json.JsonElement role in roles.GetProperty("data").EnumerateArray())
        {
            Assert.NotEqual(Guid.Empty, role.GetProperty("id").GetGuid());
            Assert.False(string.IsNullOrWhiteSpace(role.GetProperty("name").GetString()));
            foreach (System.Text.Json.JsonElement permission in role.GetProperty("permissions").EnumerateArray())
            {
                Assert.NotEqual(Guid.Empty, permission.GetProperty("id").GetGuid());
                Assert.StartsWith("auth.", permission.GetProperty("code").GetString());
                Assert.Equal("Auth", permission.GetProperty("module").GetString());
            }
        }

        var staff = await app.DbAsync(db => db.Roles.Where(r => r.Name == UserRoleName.STAFF).Select(r => r.Id).SingleAsync());
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put, $"/api/admin/rbac/users/{customer}/roles", token, new { roleIds = new[] { staff } })), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", customerToken)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/customers", customerToken)), HttpStatusCode.OK);
        var newToken = await app.LoginAsync("customer@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/customers", newToken)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put, $"/api/admin/rbac/users/{tech}/roles", token, new { roleIds = new[] { staff } })), HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Permission_data_is_preserved_but_does_not_override_role_access()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("tech@example.com", UserRoleName.TECHNICAL_ADMIN);
        await app.SeedUserAsync("staff@example.com", UserRoleName.STAFF);
        var techToken = await app.LoginAsync("tech@example.com");
        var staffToken = await app.LoginAsync("staff@example.com");
        var staffRole = await app.DbAsync(db => db.Roles.Where(r => r.Name == UserRoleName.STAFF).Select(r => r.Id).SingleAsync());
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put, $"/api/admin/rbac/roles/{staffRole}/permissions", techToken, new { permissionIds = new[] { Guid.NewGuid() } })), HttpStatusCode.BadRequest);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/customers", staffToken)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put, $"/api/admin/rbac/roles/{staffRole}/permissions", techToken, new { permissionIds = Array.Empty<Guid>() })), HttpStatusCode.OK);
        Assert.Equal(0, await app.DbAsync(db => db.RolePermissions.CountAsync(item => item.RoleId == staffRole)));
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/customers", staffToken)), HttpStatusCode.OK);
    }

    [Fact]
    public async Task Customer_with_administrative_permissions_is_still_denied_by_role()
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("tech@example.com", UserRoleName.TECHNICAL_ADMIN);
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        string techToken = await app.LoginAsync("tech@example.com");
        string customerToken = await app.LoginAsync("customer@example.com");
        Guid customerRole = await app.DbAsync(db => db.Roles.Where(role => role.Name == UserRoleName.CUSTOMER)
            .Select(role => role.Id).SingleAsync());
        Guid permission = await app.DbAsync(db => db.Permissions.Where(item => item.Code == AuthPermission.CUSTOMERS_READ)
            .Select(item => item.Id).SingleAsync());
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put,
            $"/api/admin/rbac/roles/{customerRole}/permissions", techToken,
            new { permissionIds = new List<Guid> { permission } })), HttpStatusCode.OK);
        Assert.Equal(1, await app.DbAsync(db => db.RolePermissions.CountAsync(item => item.RoleId == customerRole)));
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            "/api/admin/users/customers", customerToken)), HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrative_lock_rejects_jwt_and_password_reset_does_not_remove_manual_lock()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("staff@example.com", UserRoleName.STAFF);
        var customer = await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        var customerToken = await app.LoginAsync("customer@example.com");
        var staffToken = await app.LoginAsync("staff@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, $"/api/admin/users/{customer}/lock", staffToken, new { until = (string? )null })), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", customerToken)), HttpStatusCode.Unauthorized);
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "customer@example.com" }), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.PutAsJsonAsync("/api/auth/reset-password", new { email = "customer@example.com", token = app.Mail.Reset["customer@example.com"], newPassword = "New!Password123", confirmPassword = "New!Password123" }), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/login", new { email = "customer@example.com", password = "New!Password123" }), HttpStatusCode.Forbidden);
        Assert.Equal(UserStatus.BLOCKED, await app.DbAsync(db => db.Users.Where(u => u.Id == customer).Select(u => u.Status).SingleAsync()));
    }

    [Fact]
    public async Task Bootstrap_creates_only_the_configured_technical_admin()
    {
        await using var app = await AuthTestApp.CreateAsync(new() { ["AUTH:Bootstrap:Email"] = "bootstrap@example.com", ["AUTH:Bootstrap:Password"] = Password });
        var token = await app.LoginAsync("bootstrap@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/rbac/roles", token)), HttpStatusCode.OK);
        Assert.Equal(1, await app.DbAsync(db => db.Users.CountAsync()));
    }
}
