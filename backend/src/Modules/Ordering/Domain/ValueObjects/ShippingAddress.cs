using OnlineShop.BuildingBlocks.Domain;

namespace OnlineShop.Modules.Ordering.Domain.ValueObjects;

/// <summary>
/// Captured once at checkout and frozen on the Order from then on — like OrderDetail's price
/// snapshot, a later change to where the buyer lives must never rewrite history for an order
/// already placed. No separate "address book" concept; the buyer just types it in each checkout.
/// </summary>
public sealed class ShippingAddress : ValueObject
{
    public string RecipientName { get; }
    public string PhoneNumber { get; }
    public string AddressLine { get; }
    public string City { get; }

    private ShippingAddress(string recipientName, string phoneNumber, string addressLine, string city)
    {
        RecipientName = recipientName;
        PhoneNumber = phoneNumber;
        AddressLine = addressLine;
        City = city;
    }

    public static ShippingAddress Of(string recipientName, string phoneNumber, string addressLine, string city)
    {
        Guard.AgainstNullOrWhiteSpace(recipientName, nameof(recipientName));
        Guard.AgainstNullOrWhiteSpace(phoneNumber, nameof(phoneNumber));
        Guard.AgainstNullOrWhiteSpace(addressLine, nameof(addressLine));
        Guard.AgainstNullOrWhiteSpace(city, nameof(city));

        return new ShippingAddress(recipientName.Trim(), phoneNumber.Trim(), addressLine.Trim(), city.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return RecipientName;
        yield return PhoneNumber;
        yield return AddressLine;
        yield return City;
    }

    public override string ToString() => $"{RecipientName}, {PhoneNumber} — {AddressLine}, {City}";
}
