using System;
using System.Collections.Generic;
using TechSalesManagement.Domain.Enums;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class UserDbModel
{
    public Guid id { get; set; }
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public UserStatus status { get; set; } = UserStatus.PENDING;
    public int failed_login_attempts { get; set; }
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }
    public DateTimeOffset? last_failed_at { get; set; }
    public DateTimeOffset? locked_until { get; set; }

    // Navigation collections
    public ICollection<UserRoleDbModel> user_roles { get; set; } = new HashSet<UserRoleDbModel>();
    public UserProfileDbModel? user_profile { get; set; }
    public ICollection<UserTokenDbModel> user_tokens { get; set; } = new HashSet<UserTokenDbModel>();
    public ICollection<ShippingAddressDbModel> shipping_addresses { get; set; } = new HashSet<ShippingAddressDbModel>();
    public ICollection<OrderDbModel> orders { get; set; } = new HashSet<OrderDbModel>();
    public CartDbModel? cart { get; set; }
    public ICollection<ReviewDbModel> reviews { get; set; } = new HashSet<ReviewDbModel>();
    public ICollection<ReviewResponseDbModel> review_responses { get; set; } = new HashSet<ReviewResponseDbModel>();
    public ICollection<NotificationDbModel> notifications { get; set; } = new HashSet<NotificationDbModel>();
    public ICollection<AuditLogDbModel> audit_logs { get; set; } = new HashSet<AuditLogDbModel>();
}
