namespace OnlineShop.BuildingBlocks.Domain;

/// <summary>Fail-fast checks for enforcing domain invariants in Entities/Value Objects/Aggregates.</summary>
public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' cannot be null or whitespace.", paramName);
        return value;
    }

    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    public static decimal AgainstNegativeOrZero(decimal value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' must be greater than zero.");
        return value;
    }

    public static int AgainstNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"'{paramName}' cannot be negative.");
        return value;
    }
}
