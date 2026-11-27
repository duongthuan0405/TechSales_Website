namespace CatalogModule.Domain.Rules;

internal static class BrandRules
{
    private const int NameMaxLength = 100;

    public static bool ValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length <= NameMaxLength;
    }
}
