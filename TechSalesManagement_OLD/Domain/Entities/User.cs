using System;
using TechSalesManagement.Domain.Common;
using TechSalesManagement.Domain.Enums;
using TechSalesManagement.Common;

namespace TechSalesManagement.Domain.Entities;

public class User
{
    public Guid id { get; set; }
    public DateTimeOffset createdAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? updatedAt { get; set; }
    private string _email = string.Empty;
    private string _password = string.Empty;
    private UserStatus _status;
    private int _failed_login_attempts;
    private DateTimeOffset? _last_failed_at;
    private DateTimeOffset? _locked_until;

    // Navigation Properties
    private UserProfile? _profile;
    private List<Role> _roles = new();
    private List<ShippingAddress> _addresses = new();
    private List<Order> _orders = new();

    public string email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                throw new ArgumentException(DomainErrors.User.EmailInvalid);
            _email = value;
        }
    }

    public string password
    {
        get => _password;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(DomainErrors.User.PasswordRequired);
            _password = value;
        }
    }

    public UserStatus status
    {
        get => _status;
        set => _status = value;
    }

    public int failedLoginAttempts
    {
        get => _failed_login_attempts;
        set => _failed_login_attempts = value < 0 ? 0 : value;
    }

    public DateTimeOffset? lastFailedAt
    {
        get => _last_failed_at;
        set => _last_failed_at = value;
    }

    public DateTimeOffset? lockedUntil
    {
        get => _locked_until;
        set => _locked_until = value;
    }

    public UserProfile? profile
    {
        get => _profile;
        set => _profile = value;
    }

    public List<Role> roles
    {
        get => _roles;
        set => _roles = value ?? new();
    }

    public List<ShippingAddress> addresses
    {
        get => _addresses;
        set => _addresses = value ?? new();
    }

    public List<Order> orders
    {
        get => _orders;
        set => _orders = value ?? new();
    }

    public User(string email, string password)
    {
        this.email = email;
        this.password = password;
        this.status = UserStatus.PENDING;
        this.failedLoginAttempts = 0;
    }

    public User() { }

    public void LockAccount(DateTimeOffset? until = null)
    {
        this.status = UserStatus.BLOCKED;
        this.lockedUntil = until;
        this.updatedAt = DateTimeOffset.UtcNow;
    }

    public void UnlockAccount()
    {
        this.status = UserStatus.ACTIVE;
        this.lockedUntil = null;
        this.updatedAt = DateTimeOffset.UtcNow;
    }
}
