using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CommonModule.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using ShippingModule.Domain.Entities;
using Xunit;

namespace ShippingModule.Tests;

public sealed class ShippingFeeTests
{
    [Theory]
    [InlineData("/api/shipping/provinces/11/fee")]
    [InlineData("/api/shipping/wards/00001/fee")]
    public async Task Configured_fee_is_public_and_returned_without_a_fallback(string route)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, route);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("11", body!["data"]!["provinceCode"]!.GetValue<string>());
        Assert.Equal(10_000L, body["data"]!["fee"]!.GetValue<long>());
    }

    [Theory]
    [InlineData("/api/shipping/provinces/11/fee")]
    [InlineData("/api/shipping/wards/00001/fee")]
    public async Task Missing_configuration_means_delivery_is_not_supported(string route)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        await app.WithDatabaseAsync(async db =>
        {
            await db.ShippingFees.Where(fee => fee.ProviceCode == "11").ExecuteDeleteAsync();
            return true;
        });
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, route);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("SHIPPING.DELIVERY_NOT_SUPPORTED", body!["code"]!.GetValue<string>());
    }

    [Fact]
    public async Task Zero_fee_is_supported_and_is_not_confused_with_missing_configuration()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        await app.WithDatabaseAsync(async db =>
        {
            await db.ShippingFees.Where(fee => fee.ProviceCode == "11")
                .ExecuteUpdateAsync(update => update.SetProperty(fee => fee.Fee, 0L));
            return true;
        });
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/shipping/wards/00001/fee");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(0L, body!["data"]!["fee"]!.GetValue<long>());
    }

    [Theory]
    [InlineData("/api/shipping/provinces/99/fee", HttpStatusCode.NotFound)]
    [InlineData("/api/shipping/wards/99999/fee", HttpStatusCode.NotFound)]
    [InlineData("/api/shipping/provinces/12345678/fee", HttpStatusCode.BadRequest)]
    public async Task Unknown_or_invalid_locations_are_rejected(string route, HttpStatusCode expected)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, route);
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public void Domain_rejects_negative_fees()
    {
        Assert.Throws<DomainException>(() => new ShippingFee("11", -1L));
    }
}
