using System.Net;
using System.Text.Json;
using AuthModule.Application.Constants;
using Xunit;
using static AuthModule.Tests.AuthTestApp;

namespace AuthModule.Tests;

public sealed class UserRoleFilterTests
{
    [Theory]
    [InlineData("Business Admin", "Staff")]
    [InlineData("Technical Admin", "Staff")]
    [InlineData("Technical Admin", "Business Admin")]
    [InlineData("Technical Admin", "Technical Admin")]
    public async Task Permitted_filter_is_applied_in_database_before_count_and_paging(string actorRole, string filter)
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("actor@example.com", actorRole);
        await app.SeedUserAsync("staff@example.com", UserRoleName.STAFF);
        await app.SeedUserAsync("other-staff@example.com", UserRoleName.STAFF);
        await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        await app.SeedUserAsync("customer@example.com", UserRoleName.CUSTOMER);
        string token = await app.LoginAsync("actor@example.com");
        using HttpResponseMessage response = await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            "/api/admin/users/staff?pageSize=1&role=" + Uri.EscapeDataString(" " + filter.ToLowerInvariant() + " "), token));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement data = json.RootElement.GetProperty("data");
        Assert.Equal(filter == "Staff" ? 2 : 1, data.GetProperty("totalCount").GetInt32());
        JsonElement item = Assert.Single(data.GetProperty("items").EnumerateArray());
        Assert.Contains(item.GetProperty("roles").EnumerateArray(), role => role.GetString() == filter);
    }

    [Theory]
    [InlineData("Business Admin")]
    [InlineData("Technical Admin")]
    [InlineData("unknown-role")]
    [InlineData("Customer")]
    public async Task Business_admin_cannot_request_roles_outside_the_staff_endpoint_scope(string role)
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        string token = await app.LoginAsync("business@example.com");
        using HttpResponseMessage response = await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            "/api/admin/users/staff?role=" + Uri.EscapeDataString(role), token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Permitted_filter_with_no_matching_users_returns_an_empty_page()
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        string token = await app.LoginAsync("business@example.com");
        using HttpResponseMessage response = await app.Client.SendAsync(app.Authorized(HttpMethod.Get,
            "/api/admin/users/staff?role=Staff", token));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(0, json.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("data").GetProperty("items").GetArrayLength());
    }
}
