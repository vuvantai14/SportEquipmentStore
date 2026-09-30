namespace SportEquipmentStore.Core.Models.Authentication;

public sealed class AuthenticatedCustomer
{
    public int UserId { get; init; }

    public int CustomerId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string RoleName { get; init; } = string.Empty;
}
