using AuthModule.Application.Constants;
using AuthModule.Application.UseCases.CreateUser;
using AuthModule.Application.UseCases.LockUser;
using AuthModule.Domain.Enums;
using CommonModule.BusinessExceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AuthModule.Tests.AuthTestApp;

namespace AuthModule.Tests;

public sealed class AccountBusinessRulesTests
{
    [Fact]
    public async Task Business_admin_cannot_create_technical_admin_when_called_without_controller()
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        Guid actorId = await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        CreateUser_UCInput input = new CreateUser_UCInput(
            actorId, "technical@example.com", Password, new string[] { UserRoleName.TECHNICAL_ADMIN });

        BusinessException exception = await Assert.ThrowsAsync<BusinessException>(async () => await app.SendAsync(input));

        Assert.Equal(BusinessExceptionCode.FORBIDDEN, exception.Code);
        Assert.Equal(1, await app.DbAsync(db => db.Users.CountAsync()));
    }

    [Fact]
    public async Task Business_admin_can_create_staff_when_called_without_controller()
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        Guid actorId = await app.SeedUserAsync("business@example.com", UserRoleName.BUSINESS_ADMIN);
        CreateUser_UCInput input = new CreateUser_UCInput(
            actorId, "staff@example.com", Password, new string[] { UserRoleName.STAFF });

        CreateUser_UCOutput output = await app.SendAsync(input);

        Assert.Equal("staff@example.com", output.Email);
        Assert.Equal(UserRoleName.STAFF, Assert.Single(output.Roles));
        Assert.Equal(2, await app.DbAsync(db => db.Users.CountAsync()));
    }

    [Fact]
    public async Task Staff_cannot_lock_an_administrator_when_called_without_controller()
    {
        await using AuthTestApp app = await AuthTestApp.CreateAsync();
        Guid actorId = await app.SeedUserAsync("staff@example.com", UserRoleName.STAFF);
        Guid targetId = await app.SeedUserAsync("technical@example.com", UserRoleName.TECHNICAL_ADMIN);
        LockUser_UCInput input = new LockUser_UCInput(actorId, targetId, null);

        BusinessException exception = await Assert.ThrowsAsync<BusinessException>(async () => await app.SendAsync(input));

        Assert.Equal(BusinessExceptionCode.FORBIDDEN, exception.Code);
        Assert.Equal(UserStatus.ACTIVE, await app.DbAsync(db => db.Users
            .Where(user => user.Id == targetId)
            .Select(user => user.Status)
            .SingleAsync()));
    }
}
