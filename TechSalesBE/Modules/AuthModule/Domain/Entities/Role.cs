namespace AuthModule.Domain.Entities;
internal class Role
{
    private Guid _id = Guid.Empty;
    private string _name = string.Empty;
    private string _description = string.Empty;

    internal Role()
    {
        
    }

    internal Role(Guid id, string name)
    {
        _id = id;
        _name = name;
    }

    internal Guid Id 
    { 
        get => _id; 
        set => _id = value; 
    }
    internal string Name 
    { 
        get => _name; 
        set => _name = value; 
    }

    internal string Description
    {
        get => _description;
        set => _description = value;
    }
}