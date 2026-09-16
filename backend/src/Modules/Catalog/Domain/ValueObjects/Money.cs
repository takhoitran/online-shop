using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Catalog.Domain.ValueObjects;

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
        if (amount <= 0)
            throw new DomainException("Price must be greater than 0.");

        Guard.AgainstNullOrWhiteSpace(currency, nameof(currency));
        return new Money(amount, currency.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N0} {Currency}";
}
