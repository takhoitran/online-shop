using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Identity.Domain.ValueObjects;

public sealed class Username : ValueObject
{
    public const int MinLength = 4;
    public const int MaxLength = 32;

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string value)
    {
        Guard.AgainstNullOrWhiteSpace(value, nameof(value));
        var trimmed = value.Trim();

        if (trimmed.Length < MinLength || trimmed.Length > MaxLength)
            throw new DomainException($"Username must be between {MinLength} and {MaxLength} characters.");

        if (!trimmed.All(c => char.IsLetterOrDigit(c) || c is '_' or '.'))
            throw new DomainException("Username can only contain letters, digits, '_' or '.'.");

        return new Username(trimmed.ToLowerInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
