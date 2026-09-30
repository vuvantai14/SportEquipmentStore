namespace SportEquipmentStore.Core.Models.Authentication;

public sealed class CustomerRegistrationResult
{
    private CustomerRegistrationResult(CustomerRegistrationFailure failure)
    {
        Failure = failure;
    }

    public bool Succeeded => Failure == CustomerRegistrationFailure.None;

    public CustomerRegistrationFailure Failure { get; }

    public static CustomerRegistrationResult Success { get; } =
        new(CustomerRegistrationFailure.None);

    public static CustomerRegistrationResult Failed(CustomerRegistrationFailure failure)
    {
        if (failure == CustomerRegistrationFailure.None)
        {
            throw new ArgumentOutOfRangeException(nameof(failure));
        }

        return new CustomerRegistrationResult(failure);
    }
}
