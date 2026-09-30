namespace SportEquipmentStore.Data.Services;

internal static class ServiceValidation
{
    public static void RequirePositiveId(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "The identifier must be greater than zero.");
        }
    }

    public static string RequiredText(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"The value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }

    public static string? OptionalText(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"The value cannot exceed {maxLength} characters.", parameterName);
        }

        return normalized;
    }
}
