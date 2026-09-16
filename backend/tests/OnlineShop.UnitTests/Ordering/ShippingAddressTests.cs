using OnlineShop.Modules.Ordering.Domain.ValueObjects;

namespace OnlineShop.UnitTests.Ordering;

public class ShippingAddressTests
{
    [Fact]
    public void Of_WithAllFieldsProvided_TrimsEachField()
    {
        var address = ShippingAddress.Of("  Nguyen Van A  ", " 0912345678 ", " 12 Nguyen Trai ", " Hanoi ");

        Assert.Equal("Nguyen Van A", address.RecipientName);
        Assert.Equal("0912345678", address.PhoneNumber);
        Assert.Equal("12 Nguyen Trai", address.AddressLine);
        Assert.Equal("Hanoi", address.City);
    }

    [Theory]
    [InlineData("", "0900000000", "Street", "City")]
    [InlineData("Name", "", "Street", "City")]
    [InlineData("Name", "0900000000", "", "City")]
    [InlineData("Name", "0900000000", "Street", "")]
    public void Of_WithAnyMissingField_Throws(string recipientName, string phoneNumber, string addressLine, string city)
    {
        // Guard.AgainstNullOrWhiteSpace throws ArgumentException, not DomainException — this
        // value object has no length/format rules of its own, only "must be present".
        Assert.Throws<ArgumentException>(() => ShippingAddress.Of(recipientName, phoneNumber, addressLine, city));
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        var a = ShippingAddress.Of("Name", "0900000000", "Street", "City");
        var b = ShippingAddress.Of("Name", "0900000000", "Street", "City");

        Assert.Equal(a, b);
    }
}
