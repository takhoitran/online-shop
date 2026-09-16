using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.ValueObjects;

/// <summary>
/// Module-local — Domain layers never reference each other's Domain layer (only Application
/// layers may cross module boundaries), so this small VO is duplicated rather than shared with
/// Catalog.Domain.ValueObjects.Money. Same shape, same rules, different Bounded Context.
/// </summary>
public sealed class Money : ValueObject
{
    public const string DefaultCurrency = "VND";

    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative.");

        Guard.AgainstNullOrWhiteSpace(currency, nameof(currency));
        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = DefaultCurrency) => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot add two Money values with different currencies.");
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>Clamped at zero — a voucher discount can never make an order total negative.</summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot subtract two Money values with different currencies.");
        return new Money(Math.Max(0, Amount - other.Amount), Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N0} {Currency}";
}
