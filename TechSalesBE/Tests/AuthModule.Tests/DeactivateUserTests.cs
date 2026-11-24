using System.Net;
using System.Net.Http.Json;
using AuthModule.Application.UseCases.DeactivateUser;
using AuthModule.Application.UseCases.UnlockUser;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using CommonModule.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AuthModule.Tests.AuthTestApp;

namespace AuthModule.Tests;

public sealed class DeactivateUserTests
{
    [Theory]
    [InlineData("Technical Admin", "Customer")]
    [InlineData("Technical Admin", "Staff")]
    [InlineData("Technical Admin", "Business Admin")]
    [InlineData("Technical Admin", "Technical Admin")]
    [InlineData("Business Admin", "Customer")]
    [InlineData("Business Admin", "Staff")]
    public async Task AdministratorsCanDeactivateOnlyTheirAllowedAccountTypes(string actorRole, string targetRole)
    {
        await using var app = await AuthTestApp.CreateAsync();
        Guid actor = await app.SeedUserAsync("actor@example.com", actorRole);
        Guid target = await app.SeedUserAsync("target@example.com", targetRole);
        await app.SendAsync(new DeactivateUser_UCInput(actor, target));
        Assert.Equal(UserStatus.INACTIVE, await app.DbAsync(db => db.Users.Where(user => user.Id == target).Select(user => user.Status).SingleAsync()));
    }

    [Theory]
    [InlineData("PENDING")]
    [InlineData("ACTIVE")]
    [InlineData("BLOCKED")]
    [InlineData("INACTIVE")]
    public async Task CanDeactivateFromEveryStateAndDoesNotUnlockInactiveAccounts(string initialStatus)
    {
        await using var app = await AuthTestApp.CreateAsync();
        Guid actor = await app.SeedUserAsync("actor@example.com", "Technical Admin");
        Guid target = await app.SeedUserAsync("target@example.com", "Customer");
        await app.DbAsync(async db => {
            var user = await db.Users.SingleAsync(user => user.Id == target);
            user.Status = Enum.Parse<UserStatus>(initialStatus);
            user.LockedUntil = DateTimeOffset.UtcNow.AddHours(1);
            await db.SaveChangesAsync();
            return true;
        });
        await app.SendAsync(new DeactivateUser_UCInput(actor, target));
        var state = await app.DbAsync(db => db.Users.Where(user => user.Id == target).Select(user => new { user.Status, user.LockedUntil }).SingleAsync());
        Assert.Equal(UserStatus.INACTIVE, state.Status);
        Assert.Null(state.LockedUntil);
        await Assert.ThrowsAsync<DomainException>(() => app.SendAsync(new UnlockUser_UCInput(actor, target)));
    }

    [Theory]
    [InlineData("Business Admin", "Business Admin")]
    [InlineData("Business Admin", "Technical Admin")]
    [InlineData("Staff", "Customer")]
    [InlineData("Customer", "Customer")]
    public async Task RulesRejectForbiddenTargetsEvenWhenBypassingController(string actorRole, string targetRole)
    {
        await using var app = await AuthTestApp.CreateAsync();
        Guid actor = await app.SeedUserAsync("actor@example.com", actorRole);
        Guid target = await app.SeedUserAsync("target@example.com", targetRole);
        var error = await Assert.ThrowsAsync<BusinessException>(() => app.SendAsync(new DeactivateUser_UCInput(actor, target)));
        Assert.Equal(BusinessExceptionCode.FORBIDDEN, error.Code);
        Assert.Equal(UserStatus.ACTIVE, await app.DbAsync(db => db.Users.Where(user => user.Id == target).Select(user => user.Status).SingleAsync()));
    }

    [Theory]
    [InlineData("Business Admin")]
    [InlineData("Technical Admin")]
    public async Task CannotDeactivateSelf(string role)
    {
        await using var app = await AuthTestApp.CreateAsync();
        Guid actor = await app.SeedUserAsync("actor@example.com", role);
        var error = await Assert.ThrowsAsync<BusinessException>(() => app.SendAsync(new DeactivateUser_UCInput(actor, actor)));
        Assert.Equal(BusinessExceptionCode.BAD_REQUEST, error.Code);
    }

    [Fact]
    public async Task EndpointRejectsStaffAndBlocksExistingTokenAfterDeactivation()
    {
        await using var app = await AuthTestApp.CreateAsync();
        await app.SeedUserAsync("admin@example.com", "Technical Admin");
        await app.SeedUserAsync("staff@example.com", "Staff");
        Guid target = await app.SeedUserAsync("target@example.com", "Customer");
        string adminToken = await app.LoginAsync("admin@example.com");
        string staffToken = await app.LoginAsync("staff@example.com");
        string targetToken = await app.LoginAsync("target@example.com");
        string path = $"/api/admin/users/{target}/deactivate";
        await ExpectAsync(await app.Client.PostAsync(path, null), HttpStatusCode.Unauthorized);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, path, staffToken)), HttpStatusCode.Forbidden);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Post, path, adminToken)), HttpStatusCode.OK);
        await ExpectAsync(await app.Client.SendAsync(app.Authorized(HttpMethod.Get, "/api/user/me", targetToken)), HttpStatusCode.Unauthorized);
        await ExpectAsync(await app.Client.PostAsJsonAsync("/api/auth/login", new { email = "target@example.com", password = Password }), HttpStatusCode.Unauthorized);
    }
}
