namespace ShippingModule.Domain.Entities;

internal sealed class Ward
{
    public string Code { get; }
    public string? Name { get; }
    public Province Province { get; }

    public Ward(string code, string? name, Province province)
    {
        Code = code;
        Name = name;
        Province = province;
    }
}
