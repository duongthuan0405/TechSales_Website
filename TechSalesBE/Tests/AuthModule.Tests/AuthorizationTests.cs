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
    [Theory]
    [InlineData("/api/admin/users", UserRoleName.STAFF)]
    [InlineData("/api/admin/users/staff", UserRoleName.STAFF)]
    [InlineData("/api/admin/users/customers", UserRoleName.CUSTOMER)]
    public async Task Directory_can_search_by_partial_phone_number(string path, string role)
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("phone-admin@example.com", UserRoleName.TECHNICAL_ADMIN);
        var target = await app.SeedUserAsync("phone-target@example.com", role);
        await app.SeedUserAsync("phone-other@example.com", role);
        await app.DbAsync(async db =>
        {
            var profile = await db.UserProfiles.SingleAsync(p => p.UserId == target);
            profile.PhoneNumber = "0905678912";
            return await db.SaveChangesAsync();
        });
        var token = await app.LoginAsync("phone-admin@example.com");
        var response = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            path + "?keyword=56789&pageNumber=1&pageSize=1", token)), HttpStatusCode.OK);
        var data = response.GetProperty("data");
        Assert.Equal(1, data.GetProperty("totalCount").GetInt32());
        var item = Assert.Single(data.GetProperty("items").EnumerateArray());
        Assert.Equal(target.ToString(), item.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Technical_directory_pages_all_roles_and_applies_search_and_role_in_database()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("tech-directory@example.com", UserRoleName.TECHNICAL_ADMIN);
        await app.SeedUserAsync("match-customer@example.com", UserRoleName.CUSTOMER);
        await app.SeedUserAsync("match-staff@example.com", UserRoleName.STAFF);
        await app.SeedUserAsync("business-directory@example.com", UserRoleName.BUSINESS_ADMIN);
        var token = await app.LoginAsync("tech-directory@example.com");
        var ids = new HashSet<string>();
        for (int page = 1; page <= 2; page++)
        {
            var response = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
                $"/api/admin/users?pageNumber={page}&pageSize=1&keyword=MATCH", token)), HttpStatusCode.OK);
            var data = response.GetProperty("data");
            Assert.Equal(2, data.GetProperty("totalCount").GetInt32());
            var item = Assert.Single(data.GetProperty("items").EnumerateArray());
            Assert.True(ids.Add(item.GetProperty("id").GetString()!));
        }
        var filtered = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            "/api/admin/users?role=Customer&keyword=match", token)), HttpStatusCode.OK);
        Assert.Equal(1, filtered.GetProperty("data").GetProperty("totalCount").GetInt32());
        var businessToken = await app.LoginAsync("business-directory@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users", businessToken)), HttpStatusCode.Forbidden);
        var staffToken = await app.LoginAsync("match-staff@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users", staffToken)), HttpStatusCode.Forbidden);
        var invalidPage = await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users?pageNumber=0", token));
        Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
    }

    [Theory]
    [InlineData("customers", UserRoleName.CUSTOMER)]
    [InlineData("staff", UserRoleName.STAFF)]
    public async Task Directory_search_filters_before_counting_and_paging(string directory, string role)
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("directory-admin@example.com", UserRoleName.BUSINESS_ADMIN);
        var token = await app.LoginAsync("directory-admin@example.com");
        await app.SeedUserAsync("match-one@example.com", role);
        await app.SeedUserAsync("match-two@example.com", role);
        await app.SeedUserAsync("other@example.com", role);
        var first = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            $"/api/admin/users/{directory}?pageNumber=1&pageSize=1&keyword=MATCH", token)), HttpStatusCode.OK);
        var second = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            $"/api/admin/users/{directory}?pageNumber=2&pageSize=1&keyword=MATCH", token)), HttpStatusCode.OK);
        Assert.Equal(2, first.GetProperty("data").GetProperty("totalCount").GetInt32());
        var a = Assert.Single(first.GetProperty("data").GetProperty("items").EnumerateArray());
        var b = Assert.Single(second.GetProperty("data").GetProperty("items").EnumerateArray());
        Assert.NotEqual(a.GetProperty("id").GetString(), b.GetProperty("id").GetString());
        Assert.StartsWith("match-", a.GetProperty("email").GetString());
        Assert.StartsWith("match-", b.GetProperty("email").GetString());
    }

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
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = "illegal@example.com", password = Password, fullName = "Test User", phone = NextPhone(), roles = new[] { UserRoleName.TECHNICAL_ADMIN } })), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, $"/api/admin/users/{customer}/unlock", token)), HttpStatusCode.OK);
    }

    [Fact]
    public async Task Business_admin_can_create_staff_and_manage_status_but_cannot_edit_account_details()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        var token = await app.LoginAsync("business@example.com");
        foreach (var role in new[]
        {
            UserRoleName.CUSTOMER
        }

        )
        {
            await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = role.Replace(" ", "") + "@example.com", password = Password, fullName = "Test User", phone = NextPhone(), roles = new[] { role } })), HttpStatusCode.Forbidden);
        }

        var created = await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token,
            new { email = "new-staff@example.com", password = Password, fullName = "Test User", phone = NextPhone(), roles = new[] { UserRoleName.STAFF } })), HttpStatusCode.Created);
        Assert.Equal(UserRoleName.STAFF, Assert.Single(created.GetProperty("data").GetProperty("roles").EnumerateArray()).GetString());
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token,
            new { email = "mixed-staff-customer@example.com", password = Password, fullName = "Test User", phone = NextPhone(), roles = new[] { UserRoleName.STAFF, UserRoleName.CUSTOMER } })), HttpStatusCode.Forbidden);

        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = "illegal@example.com", password = Password, fullName = "Test User", phone = NextPhone(), roles = new[] { UserRoleName.TECHNICAL_ADMIN } })), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = "illegal-business@example.com", password = Password, fullName = "Test User", phone = NextPhone(), roles = new[] { UserRoleName.BUSINESS_ADMIN } })), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token, new { email = "illegal-mixed@example.com", password = Password, fullName = "Test User", phone = NextPhone(), roles = new[] { UserRoleName.STAFF, UserRoleName.BUSINESS_ADMIN } })), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/staff", token)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/rbac/permissions", token)), HttpStatusCode.Forbidden);
        var target = await app.SeedUserAsync("readonly-target@example.com", UserRoleName.CUSTOMER);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/users/customers", token)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Put, $"/api/admin/users/{target}", token, new { email = "changed@example.com" })), HttpStatusCode.Forbidden);
        foreach (string action in new[] { "lock", "unlock", "deactivate" })
        {
            await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, $"/api/admin/users/{target}/{action}", token, new { until = (DateTimeOffset?)null })), HttpStatusCode.OK);
        }
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
        await using var app = await AuthTestApp.CreateAsync(new() { ["AUTH:Bootstrap:Email"] = "bootstrap@example.com", ["AUTH:Bootstrap:Password"] = Password,
            ["AUTH:Bootstrap:FullName"] = "Initial Administrator", ["AUTH:Bootstrap:Phone"] = NextPhone() });
        var token = await app.LoginAsync("bootstrap@example.com");
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/admin/rbac/roles", token)), HttpStatusCode.OK);
        Assert.Equal(1, await app.DbAsync(db => db.Users.CountAsync()));
    }
}
