using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CommonModule.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using PromotionModule.Domain.Entities;
using PromotionModule.Infrastructure.Persistence.Models;
using Xunit;

namespace PromotionModule.Tests;

public sealed class VoucherManagementTests
{
    private static object Request(string code = "SALE10", string type = "PERCENT", long value = 10,
        int? maxUsage = 20, long? minOrderAmount = 0, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        return new { code, type, value, maxUsage, minOrderAmount, startDate, endDate };
    }

    private static async Task<JsonNode> Data(HttpResponseMessage response)
    {
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        return body!["data"]!;
    }

    private static async Task<Guid> Create(PromotionTestApp app, string code = "SALE10")
    {
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(),
            Request(code), "Business Admin");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        JsonNode data = await Data(response);
        return data["id"]!.GetValue<Guid>();
    }

    [Theory]
    [InlineData("Business Admin")]
    [InlineData("Technical Admin")]
    public async Task Administrators_can_create_and_read_normalized_voucher(string role)
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(),
            Request(" sale10 "), role);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        JsonNode data = await Data(response);
        Assert.Equal("SALE10", data["code"]!.GetValue<string>());
        Assert.Equal(0, data["usedCount"]!.GetValue<int>());
        Assert.True(data["isActive"]!.GetValue<bool>());
        HttpResponseMessage detail = await app.SendAsync(HttpMethod.Get, response.Headers.Location!.ToString(), Guid.NewGuid(), role: role);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Customer")]
    public async Task Non_admin_roles_are_denied_all_management_operations(string role)
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        Guid id = await Create(app);
        foreach ((HttpMethod method, string route, object? body) in new List<(HttpMethod, string, object?)>
        {
            (HttpMethod.Get, "/api/admin/vouchers", null),
            (HttpMethod.Get, $"/api/admin/vouchers/{id}", null),
            (HttpMethod.Post, "/api/admin/vouchers", Request("OTHER")),
            (HttpMethod.Put, $"/api/admin/vouchers/{id}", Request()),
            (HttpMethod.Delete, $"/api/admin/vouchers/{id}", null)
        })
        {
            HttpResponseMessage response = await app.SendAsync(method, route, Guid.NewGuid(), body, role);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    [Fact]
    public async Task Anonymous_request_is_unauthorized()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/admin/vouchers");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Duplicate_code_is_case_insensitive_and_does_not_insert_a_second_row()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        await Create(app);
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(),
            Request(" sale10 "), "Business Admin");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, await app.WithDatabaseAsync(db => db.Vouchers.CountAsync()));
    }

    [Theory]
    [InlineData("PERCENT", 101L, 20, 0L)]
    [InlineData("FIXED", 0L, 20, 0L)]
    [InlineData("FIXED", -1L, 20, 0L)]
    [InlineData("PERCENT", 10L, 0, 0L)]
    [InlineData("PERCENT", 10L, 20, -1L)]
    [InlineData("UNKNOWN", 10L, 20, 0L)]
    public async Task Invalid_definitions_are_rejected_by_validation(string type, long value, int maxUsage, long minOrderAmount)
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(),
            Request(type: type, value: value, maxUsage: maxUsage, minOrderAmount: minOrderAmount), "Technical Admin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Vouchers.CountAsync()));
    }

    [Fact]
    public async Task Invalid_date_range_and_missing_required_value_are_rejected()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(),
            Request(startDate: now, endDate: now.AddDays(-1)), "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        HttpResponseMessage missing = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(),
            new { code = "MISSING", type = "FIXED" }, "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
    }

    [Fact]
    public async Task Update_preserves_used_count_and_rejects_duplicate_code_or_lower_usage_limit()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        Guid id = await Create(app);
        await Create(app, "TAKEN");
        await app.WithDatabaseAsync(async db =>
        {
            await db.Vouchers.Where(row => row.Id == id).ExecuteUpdateAsync(update => update.SetProperty(row => row.UsedCount, 5));
            return true;
        });
        HttpResponseMessage duplicate = await app.SendAsync(HttpMethod.Put, $"/api/admin/vouchers/{id}", Guid.NewGuid(),
            Request("TAKEN"), "Business Admin");
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        HttpResponseMessage tooSmall = await app.SendAsync(HttpMethod.Put, $"/api/admin/vouchers/{id}", Guid.NewGuid(),
            Request(maxUsage: 4), "Business Admin");
        Assert.Equal(HttpStatusCode.Conflict, tooSmall.StatusCode);
        HttpResponseMessage updated = await app.SendAsync(HttpMethod.Put, $"/api/admin/vouchers/{id}", Guid.NewGuid(),
            Request("RENAMED", "FIXED", 25000, maxUsage: null, minOrderAmount: null), "Technical Admin");
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        JsonNode data = await Data(updated);
        Assert.Equal(5, data["usedCount"]!.GetValue<int>());
        Assert.Equal("RENAMED", data["code"]!.GetValue<string>());
        Assert.Null(data["maxUsage"]);
    }

    [Fact]
    public async Task Deactivation_keeps_voucher_usage_and_order_links_and_is_idempotent()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        Guid id = await Create(app);
        await app.WithDatabaseAsync(async db =>
        {
            db.OrderVouchers.Add(new OrderVoucherDbModel { OrderId = Guid.NewGuid(), VoucherId = id });
            await db.SaveChangesAsync();
            return true;
        });
        for (int attempt = 0; attempt < 2; attempt++)
        {
            HttpResponseMessage response = await app.SendAsync(HttpMethod.Delete, $"/api/admin/vouchers/{id}", Guid.NewGuid(), role: "Technical Admin");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        VoucherDbModel row = await app.WithDatabaseAsync(db => db.Vouchers.AsNoTracking().SingleAsync());
        Assert.False(row.IsActive);
        Assert.Equal(1, await app.WithDatabaseAsync(db => db.OrderVouchers.CountAsync()));
        await app.SendAsync(HttpMethod.Put, $"/api/admin/vouchers/{id}", Guid.NewGuid(), Request(value: 15), "Business Admin");
        Assert.False(await app.WithDatabaseAsync(db => db.Vouchers.Where(item => item.Id == id).Select(item => item.IsActive).SingleAsync()));
    }

    [Fact]
    public async Task Pagination_includes_inactive_vouchers_and_reports_full_count()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        Guid first = await Create(app, "FIRST");
        await Create(app, "SECOND");
        await Create(app, "THIRD");
        await app.SendAsync(HttpMethod.Delete, $"/api/admin/vouchers/{first}", Guid.NewGuid(), role: "Business Admin");
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/admin/vouchers?pageNumber=2&pageSize=2", Guid.NewGuid(), role: "Business Admin");
        JsonNode data = await Data(response);
        Assert.Equal(3, data["totalCount"]!.GetValue<int>());
        Assert.Single(data["items"]!.AsArray());
        HttpResponseMessage invalidPage = await app.SendAsync(HttpMethod.Get, "/api/admin/vouchers?pageSize=0", Guid.NewGuid(), role: "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
    }

    [Fact]
    public async Task Unknown_voucher_returns_not_found()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        Guid id = Guid.NewGuid();
        foreach (HttpMethod method in new List<HttpMethod> { HttpMethod.Get, HttpMethod.Put, HttpMethod.Delete })
        {
            HttpResponseMessage response = await app.SendAsync(method, $"/api/admin/vouchers/{id}", Guid.NewGuid(),
                method == HttpMethod.Put ? Request() : null, "Business Admin");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task Save_failure_rolls_back_creation()
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        app.SaveFailure.FailWrites = true;
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(), Request(), "Business Admin");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Vouchers.CountAsync()));
    }

    [Fact]
    public void Entity_rejects_invalid_definition_without_a_validator()
    {
        Assert.Throws<DomainException>(() => Voucher.Create("CODE", "PERCENT", 101, 10, 0, null, null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Entity_normalizes_scheduled_times_to_utc_for_postgresql()
    {
        DateTimeOffset start = new DateTimeOffset(2026, 12, 1, 7, 0, 0, TimeSpan.FromHours(7));
        DateTimeOffset end = start.AddDays(1);
        Voucher voucher = Voucher.Create("CODE", "FIXED", 10000, 10, 0, start, end, DateTimeOffset.UtcNow);
        Assert.Equal(TimeSpan.Zero, voucher.StartDate!.Value.Offset);
        Assert.Equal(start.UtcDateTime, voucher.StartDate.Value.UtcDateTime);
        voucher.Update("CODE", "FIXED", 20000, 10, 0, start, end, DateTimeOffset.UtcNow);
        Assert.Equal(TimeSpan.Zero, voucher.EndDate!.Value.Offset);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ")]
    public async Task Empty_and_oversized_codes_are_rejected(string code)
    {
        await using PromotionTestApp app = await PromotionTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Post, "/api/admin/vouchers", Guid.NewGuid(),
            Request(code), "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await app.WithDatabaseAsync(db => db.Vouchers.CountAsync()));
    }
}
