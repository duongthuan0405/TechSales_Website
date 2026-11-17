using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ShippingModule.Tests;

public sealed class ShippingFeeManagementTests
{
    [Theory]
    [InlineData("Business Admin", 15000L)]
    [InlineData("Technical Admin", 0L)]
    public async Task Administrators_can_update_fee_and_public_queries_use_the_new_value(string role, long fee)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, "/api/shipping/provinces/11/fee",
            Guid.NewGuid(), new { fee }, role);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        HttpResponseMessage lookup = await app.SendAsync(HttpMethod.Get, "/api/shipping/wards/00001/fee");
        JsonObject? body = await lookup.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(fee, body!["data"]!["fee"]!.GetValue<long>());
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Customer")]
    public async Task Non_admin_roles_cannot_change_fees(string role)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, "/api/shipping/provinces/11/fee",
            Guid.NewGuid(), new { fee = 15000L }, role);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        long fee = await app.WithDatabaseAsync(db => db.ShippingFees.Where(item => item.ProviceCode == "11")
            .Select(item => item.Fee).SingleAsync());
        Assert.Equal(10000L, fee);
    }

    [Fact]
    public async Task Unauthenticated_requests_cannot_change_fees()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, "/api/shipping/provinces/11/fee",
            body: new { fee = 15000L });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creating_missing_configuration_enables_delivery_again()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        await app.WithDatabaseAsync(async db =>
        {
            await db.ShippingFees.Where(fee => fee.ProviceCode == "11").ExecuteDeleteAsync();
            return true;
        });
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, "/api/shipping/provinces/11/fee",
            Guid.NewGuid(), new { fee = 12000L }, "Business Admin");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, await app.WithDatabaseAsync(db => db.ShippingFees.CountAsync(fee => fee.ProviceCode == "11")));
        HttpResponseMessage lookup = await app.SendAsync(HttpMethod.Get, "/api/shipping/provinces/11/fee");
        Assert.Equal(HttpStatusCode.OK, lookup.StatusCode);
    }

    [Theory]
    [InlineData("11", -1L, HttpStatusCode.BadRequest)]
    [InlineData("99", 10000L, HttpStatusCode.NotFound)]
    public async Task Invalid_updates_are_rejected(string provinceCode, long fee, HttpStatusCode expected)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/shipping/provinces/{provinceCode}/fee",
            Guid.NewGuid(), new { fee }, "Technical Admin");
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Omitting_fee_does_not_silently_enable_free_shipping()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, "/api/shipping/provinces/11/fee",
            Guid.NewGuid(), new { }, "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Storage_failure_preserves_the_previous_fee()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        app.SaveFailure.FailWrites = true;
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, "/api/shipping/provinces/11/fee",
            Guid.NewGuid(), new { fee = 20000L }, "Business Admin");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        app.SaveFailure.FailWrites = false;
        long fee = await app.WithDatabaseAsync(db => db.ShippingFees.Where(item => item.ProviceCode == "11")
            .Select(item => item.Fee).SingleAsync());
        Assert.Equal(10000L, fee);
    }
}
