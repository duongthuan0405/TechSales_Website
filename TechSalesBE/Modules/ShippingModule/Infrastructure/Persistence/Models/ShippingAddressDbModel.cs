namespace ShippingModule.Infrastructure.Persistence.Models;

internal sealed class ShippingAddressDbModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WardCode { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public bool? IsDefault { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdateAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public WardDbModel Ward { get; set; } = null!;
}
