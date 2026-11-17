namespace ShippingModule.Application.UseCases.GetProvinces;

internal sealed record GetProvinces_UCOutput(List<GetProvinces_UCOutput.ProvinceItem> Items)
{
    internal sealed record ProvinceItem(string Code, string? Name);
}
