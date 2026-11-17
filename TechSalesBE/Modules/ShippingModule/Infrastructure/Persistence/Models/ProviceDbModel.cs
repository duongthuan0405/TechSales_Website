namespace ShippingModule.Infrastructure.Persistence.Models;

internal sealed class ProvinceDbModel
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }

    public ICollection<WardDbModel> Wards { get; set; } = new HashSet<WardDbModel>();
}
