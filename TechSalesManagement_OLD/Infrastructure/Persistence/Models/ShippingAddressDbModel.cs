using System;

namespace TechSalesManagement.Infrastructure.Persistence.Models;

public class ShippingAddressDbModel
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public string province { get; set; } = string.Empty;
    public string ward { get; set; } = string.Empty;
    public string detail { get; set; } = string.Empty;
    public bool is_default { get; set; }
    public DateTimeOffset created_at { get; set; }
    public DateTimeOffset? updated_at { get; set; }
    public DateTimeOffset? deleted_at { get; set; }

    // Navigation properties
    public UserDbModel user { get; set; } = null!;
}
