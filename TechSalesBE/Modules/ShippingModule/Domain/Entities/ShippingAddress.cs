using ShippingModule.Domain.Rules;

namespace ShippingModule.Domain.Entities;

internal sealed class ShippingAddress
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public Ward Ward { get; private set; }
    public string Detail { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public ShippingAddress(Guid id, Guid userId, Ward ward, string detail,
        bool isDefault, DateTimeOffset createdAt, DateTimeOffset? updatedAt = null,
        DateTimeOffset? deletedAt = null)
    {
        ShippingAddressRules.EnsureValidIdentity(id, userId);

        ShippingAddressRules.EnsureValidAddress(ward, detail);
        Id = id;
        UserId = userId;
        Ward = ward;
        Detail = detail.Trim();
        IsDefault = isDefault;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public void Update(Ward ward, string detail, DateTimeOffset now)
    {
        ShippingAddressRules.EnsureNotDeleted(DeletedAt);
        ShippingAddressRules.EnsureValidAddress(ward, detail);
        Ward = ward;
        Detail = detail.Trim();
        UpdatedAt = now;
    }

    public void SetDefault(bool isDefault, DateTimeOffset now)
    {
        ShippingAddressRules.EnsureNotDeleted(DeletedAt);
        if (IsDefault == isDefault)
        {
            return;
        }

        IsDefault = isDefault;
        UpdatedAt = now;
    }

    public void Delete(DateTimeOffset now)
    {
        ShippingAddressRules.EnsureNotDeleted(DeletedAt);
        IsDefault = false;
        DeletedAt = now;
        UpdatedAt = now;
    }

}
