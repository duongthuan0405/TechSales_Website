namespace AuthModule.Domain.Entities;

internal class Permission
{
    private Guid _id = Guid.Empty;
    private string _code = string.Empty;
    private string? _description = null;

    internal Permission(Guid id, string code, string? description)
    {
        _id = id;
        _code = code;
        this._description = description;
    }

    internal Permission() {}

    internal Guid Id 
    { 
        get => _id; 
        set => _id = value; 
    }
    internal string Code 
    { 
        get => _code; 
        set => _code = value; 
    }
    internal string? Description 
    { 
        get => _description; 
        set => _description = value; 
    }
}