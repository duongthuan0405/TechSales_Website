using AuthModule.Domain.Enums;
using AuthModule.Domain.ErrorMessages;

namespace AuthModule.Domain.Entities;

internal class User
{
    private Guid _id = Guid.Empty;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _updatedAt = null;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private UserStatus _status = UserStatus.PENDING;
    private int _failedLoginAttempts;
    private DateTimeOffset? _lastFailedAt;
    private DateTimeOffset? _lockedUntil;

    internal Guid Id 
    { 
        get => _id; 
        set => _id = value; 
    }
    internal DateTimeOffset CreatedAt 
    { 
        get => _createdAt; 
        set => _createdAt = value; 
        } 
    internal DateTimeOffset? UpdatedAt 
    { 
        get => _updatedAt; 
        set => _updatedAt = value; 
    }

    internal string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                throw new ArgumentException(DomainErrors.User.EmailInvalid);
            _email = value;
        }
    }

    internal string Password
    {
        get => _password;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(DomainErrors.User.PasswordInvalid);
            _password = value;
        }
    }

    internal UserStatus Status
    {
        get => _status;
        set => _status = value;
    }

    internal int FailedLoginAttempts
    {
        get => _failedLoginAttempts;
        set => _failedLoginAttempts = value < 0 ? 0 : value;
    }

    internal DateTimeOffset? LastFailedAt
    {
        get => _lastFailedAt;
        set => _lastFailedAt = value;
    }

    internal DateTimeOffset? LockedUntil
    {
        get => _lockedUntil;
        set => _lockedUntil = value;
    }

    internal User(string email, string password)
    {
        this.Email = email;
        this.Password = password;
        this.Status = UserStatus.PENDING;
        this.FailedLoginAttempts = 0;
    }

    internal User() { }

    internal void LockAccount(DateTimeOffset? until = null)
    {
        this.Status = UserStatus.BLOCKED;
        this.LockedUntil = until;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }

    internal void UnlockAccount()
    {
        this.Status = UserStatus.ACTIVE;
        this.LockedUntil = null;
        this.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
