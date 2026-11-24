using System.Net;
using System.Net.Http.Json;
using AuthModule.Application.Constants;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AuthModule.Tests.AuthTestApp;

namespace AuthModule.Tests;

public sealed class ProfileCreationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Creation_requires_profile_and_rejects_duplicate_phone(bool administrator)
    {
        await using var app = await AuthTestApp.CreateAsync();
        string? token = null;
        if (administrator)
        {
            await app.SeedUserAsync("admin@example.com", UserRoleName.TECHNICAL_ADMIN);
            token = await app.LoginAsync("admin@example.com");
        }
        Task<HttpResponseMessage> Send(string email, string? name, string? phone) => administrator
            ? app.Client.SendAsync(app.Authorized(HttpMethod.Post, "/api/admin/users", token!,
                new { email, password = Password, roles = new[] { UserRoleName.STAFF }, fullName = name, phone }))
            : app.Client.PostAsJsonAsync("/api/auth/register", new { email, password = Password, confirmPassword = Password, fullName = name, phone });

        await ExpectAsync(await Send("first@example.com", "  First User  ", "0901234567"), HttpStatusCode.Created);
        var profile = await app.DbAsync(db => db.UserProfiles.SingleAsync(p => p.PhoneNumber == "0901234567"));
        Assert.Equal("First User", profile.FullName);
        var before = await app.DbAsync(db => db.Users.CountAsync());
        var duplicate = await ExpectAsync(await Send("second@example.com", "Second User", "0901234567"), HttpStatusCode.Conflict);
        Assert.Contains("Số điện thoại", duplicate.GetProperty("message").GetString());
        await ExpectAsync(await Send("second@example.com", " ", "0901234568"), HttpStatusCode.BadRequest);
        await ExpectAsync(await Send("second@example.com", "Second User", ""), HttpStatusCode.BadRequest);
        await ExpectAsync(await Send("second@example.com", null, null), HttpStatusCode.BadRequest);
        Assert.Equal(before, await app.DbAsync(db => db.Users.CountAsync()));
    }

    [Fact]
    public async Task Profile_update_rejects_another_users_phone_but_allows_own_phone()
    {
        await using var app = await AuthTestApp.CreateAsync();
        var first = await app.SeedUserAsync("first@example.com", UserRoleName.CUSTOMER);
        var second = await app.SeedUserAsync("second@example.com", UserRoleName.CUSTOMER);
        var ownPhone = await app.DbAsync(db => db.UserProfiles.Where(p => p.UserId == first).Select(p => p.PhoneNumber).SingleAsync());
        var otherPhone = await app.DbAsync(db => db.UserProfiles.Where(p => p.UserId == second).Select(p => p.PhoneNumber).SingleAsync());
        var token = await app.LoginAsync("first@example.com");
        async Task<HttpResponseMessage> Update(string phone)
        {
            var request = app.Authorized(HttpMethod.Put, "/api/user-profile", token);
            var form = new MultipartFormDataContent();
            form.Add(new StringContent("Updated Name"), "FullName");
            form.Add(new StringContent(phone), "Phone");
            request.Content = form;
            return await app.Client.SendAsync(request);
        }
        await ExpectAsync(await Update(otherPhone), HttpStatusCode.Conflict);
        Assert.Equal(ownPhone, await app.DbAsync(db => db.UserProfiles.Where(p => p.UserId == first).Select(p => p.PhoneNumber).SingleAsync()));
        await ExpectAsync(await Update(ownPhone), HttpStatusCode.OK);
        await Assert.ThrowsAsync<DbUpdateException>(() => app.DbAsync(async db =>
        {
            var profile = await db.UserProfiles.SingleAsync(p => p.UserId == second);
            profile.PhoneNumber = ownPhone;
            return await db.SaveChangesAsync();
        }));
    }
}
