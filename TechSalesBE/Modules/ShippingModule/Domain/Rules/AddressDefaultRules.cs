using ShippingModule.Domain.Entities;

namespace ShippingModule.Domain.Rules;

internal static class AddressDefaultRules
{
    internal static bool ShouldBeDefault(List<ShippingAddress> existingAddresses)
    {
        return existingAddresses.Count == 0;
    }

    internal static List<ShippingAddress> SelectDefault(List<ShippingAddress> addresses, Guid selectedId, DateTimeOffset now)
    {
        List<ShippingAddress> changed = new List<ShippingAddress>();
        foreach (ShippingAddress address in addresses)
        {
            bool shouldBeDefault = address.Id == selectedId;
            if (address.IsDefault != shouldBeDefault)
            {
                address.SetDefault(shouldBeDefault, now);
                changed.Add(address);
            }
        }

        return changed;
    }

    internal static ShippingAddress? SelectReplacement(bool deletedWasDefault,
        Guid deletedId, List<ShippingAddress> addresses, DateTimeOffset now)
    {
        if (!deletedWasDefault)
        {
            return null;
        }

        List<ShippingAddress> remaining = addresses
            .Where(address => address.Id != deletedId && !address.DeletedAt.HasValue)
            .OrderBy(address => address.CreatedAt).ThenBy(address => address.Id).ToList();
        if (remaining.Count == 0)
        {
            return null;
        }

        ShippingAddress replacement = remaining[0];
        replacement.SetDefault(true, now);
        return replacement;
    }
}
