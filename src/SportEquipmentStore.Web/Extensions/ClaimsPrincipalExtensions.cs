using System.Security.Claims;

namespace SportEquipmentStore.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public const string CustomerIdClaimType = "CustomerId";

    public static bool TryGetCustomerId(
        this ClaimsPrincipal principal,
        out int customerId)
    {
        var value = principal.FindFirstValue(CustomerIdClaimType);
        return int.TryParse(value, out customerId) && customerId > 0;
    }
}
