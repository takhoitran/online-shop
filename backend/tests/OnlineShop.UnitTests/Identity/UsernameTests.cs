using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Identity.Domain.ValueObjects;

namespace OnlineShop.UnitTests.Identity;

public class UsernameTests
{
    [Theory]
    [InlineData("john.doe")]
    [InlineData("user_99")]
    [InlineData("abcd")]
    public void Create_WithValidValue_Succeeds(string value)
    {
        var username = Username.Create(value);
        Assert.Equal(value.ToLowerInvariant(), username.Value);
    }

    [Fact]
    public void Create_NormalizesToLowercaseAndTrims()
    {
        var username = Username.Create("  JohnDoe  ");
        Assert.Equal("johndoe", username.Value);
    }

    [Fact]
    public void Create_TooShort_Throws()
    {
        Assert.Throws<DomainException>(() => Username.Create("ab"));
    }

    [Fact]
    public void Create_TooLong_Throws()
    {
        Assert.Throws<DomainException>(() => Username.Create(new string('a', Username.MaxLength + 1)));
    }

    [Theory]
    [InlineData("john doe")]
    [InlineData("john@doe")]
    [InlineData("john-doe")]
    public void Create_WithDisallowedCharacters_Throws(string value)
    {
        Assert.Throws<DomainException>(() => Username.Create(value));
    }

    [Fact]
    public void TwoUsernames_WithSameValue_AreEqual()
    {
        Assert.Equal(Username.Create("sameuser"), Username.Create("SameUser"));
    }
}
