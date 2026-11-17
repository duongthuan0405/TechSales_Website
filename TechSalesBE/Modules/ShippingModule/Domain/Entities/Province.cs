namespace ShippingModule.Domain.Entities;

internal sealed class Province
{
    public string Code { get; }
    public string? Name { get; }

    public Province(string code, string? name)
    {
        Code = code;
        Name = name;
    }
}
