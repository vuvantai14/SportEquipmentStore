using SportEquipmentStore.Core.Models.Authentication;

namespace SportEquipmentStore.Core.Interfaces;

public interface IAuthService
{
    Task<CustomerRegistrationResult> RegisterCustomerAsync(
        string username,
        string email,
        string fullName,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedCustomer?> AuthenticateCustomerAsync(
        string identifier,
        string password,
        CancellationToken cancellationToken = default);
}
