namespace ShippingModule.Application.UseCases.GetWards;

internal sealed record GetWards_UCOutput(List<GetWards_UCOutput.WardItem> Items)
{
    internal sealed record WardItem(string Code, string? Name, string ProvinceCode);
}
