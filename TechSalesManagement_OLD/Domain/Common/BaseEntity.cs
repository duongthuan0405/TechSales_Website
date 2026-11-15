using System;

namespace TechSalesManagement.Domain.Common;

public abstract class BaseEntity
{
    private Guid _id;
    private DateTimeOffset _created_at;
    private DateTimeOffset? _updated_at;

    public Guid id
    {
        get => _id;
        set => _id = value;
    }

    public DateTimeOffset created_at
    {
        get => _created_at;
        set => _created_at = value;
    }

    public DateTimeOffset? updated_at
    {
        get => _updated_at;
        set => _updated_at = value;
    }

    protected BaseEntity()
    {
        _created_at = DateTimeOffset.UtcNow;
    }
}
