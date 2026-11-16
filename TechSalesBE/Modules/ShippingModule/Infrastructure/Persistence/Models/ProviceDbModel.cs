namespace ShippingModule.Infrastructure.Persistence.Models;

internal sealed class ProviceDbModel
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }

    public ICollection<WardDbModel> Wards { get; set; } = new HashSet<WardDbModel>();
}
