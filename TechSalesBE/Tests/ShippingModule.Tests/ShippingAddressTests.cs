using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CommonModule.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using ShippingModule.Domain.Constants;
using ShippingModule.Domain.Entities;
using ShippingModule.Infrastructure.Persistence.Models;
using Xunit;

namespace ShippingModule.Tests;

public sealed class ShippingAddressTests
{
    private static async Task<JsonNode> DataAsync(HttpResponseMessage response)
    {
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(body);
        return body["data"]!;
    }

    private static async Task<Guid> CreateAsync(ShippingTestApp app, Guid actor, string detail = "123 Test Street")
    {
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/shipping-address", actor,
            new { wardCode = "00001", detail });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        JsonNode data = await DataAsync(response);
        return data["id"]!.GetValue<Guid>();
    }

    [Fact]
    public async Task Locations_are_public_and_wards_are_filtered_by_province()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage provinces = await app.SendAsync(HttpMethod.Get, "/api/shipping/provinces");
        Assert.Equal(HttpStatusCode.OK, provinces.StatusCode);
        JsonNode provinceData = await DataAsync(provinces);
        Assert.Equal(2, provinceData["items"]!.AsArray().Count);

        HttpResponseMessage wards = await app.SendAsync(HttpMethod.Get, "/api/shipping/provinces/11/wards");
        JsonNode wardData = await DataAsync(wards);
        JsonArray items = wardData["items"]!.AsArray();
        Assert.Single(items);
        Assert.Equal("00001", items[0]!["code"]!.GetValue<string>());
        Assert.Equal("11", items[0]!["provinceCode"]!.GetValue<string>());
    }

    [Fact]
    public async Task Unknown_province_returns_not_found()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/shipping/provinces/99/wards");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Address_endpoints_require_authentication()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/shipping-address");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task First_address_is_default_and_each_user_only_sees_their_own_addresses()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        Guid actor = Guid.NewGuid();
        Guid first = await CreateAsync(app, actor);
        Guid second = await CreateAsync(app, actor);
        await CreateAsync(app, Guid.NewGuid());
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/shipping-address", actor);
        JsonNode data = await DataAsync(response);
        JsonArray items = data["items"]!.AsArray();
        Assert.Equal(2, items.Count);
        Assert.Equal(first, items[0]!["id"]!.GetValue<Guid>());
        Assert.True(items[0]!["isDefault"]!.GetValue<bool>());
        Assert.Equal(second, items[1]!["id"]!.GetValue<Guid>());
        Assert.False(items[1]!["isDefault"]!.GetValue<bool>());
        Assert.Equal("11", items[0]!["provinceCode"]!.GetValue<string>());
    }

    [Fact]
    public async Task Set_default_can_be_repeated_and_keeps_exactly_one_default()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        Guid actor = Guid.NewGuid();
        await CreateAsync(app, actor);
        Guid second = await CreateAsync(app, actor);
        for (int attempt = 0; attempt < 2; attempt++)
        {
            HttpResponseMessage changed = await app.SendAsync(HttpMethod.Put, $"/api/shipping-address/{second}/default", actor);
            Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        }

        List<ShippingAddressDbModel> rows = await app.WithDatabaseAsync(db => db.ShippingAddresss.AsNoTracking().ToListAsync());
        ShippingAddressDbModel selected = Assert.Single(rows, row => row.IsDefault == true);
        Assert.Equal(second, selected.Id);
    }

    [Fact]
    public async Task Update_changes_location_and_detail_without_changing_default()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        Guid actor = Guid.NewGuid();
        Guid id = await CreateAsync(app, actor);
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/shipping-address/{id}", actor,
            new { wardCode = "00002", detail = " 456 New Street " });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        JsonNode data = await DataAsync(response);
        Assert.Equal("44", data["provinceCode"]!.GetValue<string>());
        Assert.Equal("456 New Street", data["detail"]!.GetValue<string>());
        Assert.True(data["isDefault"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("update")]
    [InlineData("default")]
    [InlineData("delete")]
    public async Task Another_user_cannot_modify_an_address(string operation)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        Guid id = await CreateAsync(app, Guid.NewGuid());
        HttpMethod method = HttpMethod.Put;
        string path = $"/api/shipping-address/{id}";
        object? body = null;
        if (operation == "update")
        {
            body = new { wardCode = "00002", detail = "Unauthorized change" };
        }
        else if (operation == "default")
        {
            path += "/default";
        }
        else
        {
            method = HttpMethod.Delete;
        }

        HttpResponseMessage response = await app.SendAsync(method, path, Guid.NewGuid(), body);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        ShippingAddressDbModel row = await app.WithDatabaseAsync(db => db.ShippingAddresss.AsNoTracking().SingleAsync());
        Assert.Null(row.DeletedAt);
        Assert.Equal("123 Test Street", row.Detail);
        Assert.True(row.IsDefault);
    }

    [Fact]
    public async Task Delete_soft_deletes_and_reassigns_default_then_recreating_starts_with_default()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        Guid actor = Guid.NewGuid();
        Guid first = await CreateAsync(app, actor);
        Guid second = await CreateAsync(app, actor);
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Delete, $"/api/shipping-address/{first}", actor);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<ShippingAddressDbModel> rows = await app.WithDatabaseAsync(db => db.ShippingAddresss.AsNoTracking().ToListAsync());
        ShippingAddressDbModel deleted = rows.Single(row => row.Id == first);
        Assert.NotNull(deleted.DeletedAt);
        Assert.False(deleted.IsDefault);
        Assert.True(rows.Single(row => row.Id == second).IsDefault);
        HttpResponseMessage listed = await app.SendAsync(HttpMethod.Get, "/api/shipping-address", actor);
        Assert.Single((await DataAsync(listed))["items"]!.AsArray());

        HttpResponseMessage repeated = await app.SendAsync(HttpMethod.Delete, $"/api/shipping-address/{first}", actor);
        Assert.Equal(HttpStatusCode.NotFound, repeated.StatusCode);
        await app.SendAsync(HttpMethod.Delete, $"/api/shipping-address/{second}", actor);
        Guid recreated = await CreateAsync(app, actor);
        ShippingAddressDbModel current = await app.WithDatabaseAsync(db => db.ShippingAddresss.AsNoTracking().SingleAsync(row => row.Id == recreated));
        Assert.True(current.IsDefault);
    }

    [Fact]
    public async Task Deleting_non_default_preserves_current_default()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        Guid actor = Guid.NewGuid();
        Guid first = await CreateAsync(app, actor);
        Guid second = await CreateAsync(app, actor);
        await app.SendAsync(HttpMethod.Delete, $"/api/shipping-address/{second}", actor);
        ShippingAddressDbModel row = await app.WithDatabaseAsync(db => db.ShippingAddresss.AsNoTracking().SingleAsync(item => item.Id == first));
        Assert.True(row.IsDefault);
    }

    [Theory]
    [InlineData("00001", " ", HttpStatusCode.BadRequest)]
    [InlineData("", "Street", HttpStatusCode.BadRequest)]
    [InlineData("99999", "Street", HttpStatusCode.NotFound)]
    public async Task Invalid_address_does_not_write_data(string wardCode, string detail, HttpStatusCode expected)
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/shipping-address", Guid.NewGuid(),
            new { wardCode, detail });
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.ShippingAddresss.CountAsync()));
    }

    [Fact]
    public async Task Save_failure_rolls_back_default_changes()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        Guid actor = Guid.NewGuid();
        Guid first = await CreateAsync(app, actor);
        Guid second = await CreateAsync(app, actor);
        app.SaveFailure.FailWrites = true;
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, $"/api/shipping-address/{second}/default", actor);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        app.SaveFailure.FailWrites = false;
        ShippingAddressDbModel selected = await app.WithDatabaseAsync(db => db.ShippingAddresss.AsNoTracking().SingleAsync(row => row.IsDefault == true));
        Assert.Equal(first, selected.Id);
    }

    [Fact]
    public void Domain_rejects_invalid_detail_and_changes_to_deleted_address()
    {
        Province province = new Province("11", "Test province");
        Ward ward = new Ward("00001", "Test ward", province);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Assert.Throws<DomainException>(() => new ShippingAddress(Guid.NewGuid(), Guid.NewGuid(), ward,
            new string('x', ShippingLimits.AddressDetailMaxLength + 1), false, now));
        ShippingAddress address = new ShippingAddress(Guid.NewGuid(), Guid.NewGuid(), ward, "Street", true, now);
        address.Delete(now);
        Assert.Throws<DomainException>(() => address.Update(ward, "Other Street", now));
        Assert.Throws<DomainException>(() => address.SetDefault(true, now));
    }
}
