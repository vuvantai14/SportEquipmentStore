namespace SportEquipmentStore.Core.Models.Authentication;

public enum CustomerRegistrationFailure
{
    None,
    DuplicateUsername,
    DuplicateEmail,
    DuplicateAccount,
    CustomerRoleUnavailable,
}
