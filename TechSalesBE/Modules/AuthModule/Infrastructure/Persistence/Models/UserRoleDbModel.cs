namespace AuthModule.Infrastructure.Persistence.Models;

internal sealed class UserRoleDbModel
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public UserDbModel User { get; set; } = null!;
    public RoleDbModel Role { get; set; } = null!;
}
