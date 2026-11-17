using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace ShippingModule.Tests;

public sealed class VietnameseResponseTests
{
    [Fact]
    public async Task Success_message_is_vietnamese()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/shipping/provinces/11/fee");
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Lấy phí vận chuyển thành công.", body!["message"]!.GetValue<string>());
    }

    [Fact]
    public async Task Default_validation_message_is_vietnamese()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Get, "/api/shipping-address", Guid.Empty);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("BAD_REQUEST", body!["code"]!.GetValue<string>());
        string error = body["data"]!["ActorId"]![0]!.GetValue<string>();
        Assert.Contains("không", error);
        Assert.DoesNotContain("must not", error);
    }

    [Fact]
    public async Task Json_binding_errors_do_not_expose_english_framework_messages()
    {
        await using ShippingTestApp app = await ShippingTestApp.CreateAsync();
        HttpResponseMessage response = await app.SendAsync(HttpMethod.Put, "/api/shipping/provinces/11/fee",
            Guid.NewGuid(), new { }, "Business Admin");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonObject? body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal("Dữ liệu yêu cầu không hợp lệ.", body!["message"]!.GetValue<string>());
        Assert.Equal("BAD_REQUEST", body["code"]!.GetValue<string>());
    }
}
