using AuthModule.Domain.Enums;

namespace AuthModule.Domain.Entities;

internal class UserToken
{
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private Guid _id = Guid.Empty;
    private DateTimeOffset? _updatedAt =  null; 
    private Guid _userId = Guid.Empty;
    private string _token = string.Empty;
    private TokenType _type;
    private DateTimeOffset _expiredAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _usedAt = null;

    internal DateTimeOffset CreatedAt { get => _createdAt; set => _createdAt = value; }
    internal Guid Id { get => _id; set => _id = value; }
    internal DateTimeOffset? UpdatedAt { get => _updatedAt; set => _updatedAt = value; }
    internal Guid UserId { get => _userId; set => _userId = value; }
    internal string Token { get => _token; set => _token = value; }
    internal TokenType Type { get => _type; set => _type = value; }
    internal DateTimeOffset ExpiredAt { get => _expiredAt; set => _expiredAt = value; }
    internal DateTimeOffset? UsedAt { get => _usedAt; set => _usedAt = value; }

    internal UserToken(Guid userId, string token, TokenType type, DateTimeOffset expiredAt)
    {
        this.UserId = userId;
        this.Token = token;
        this.Type = type;
        this.ExpiredAt = expiredAt;
    }

    internal UserToken() { }
}
