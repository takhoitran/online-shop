using OnlineShop.BuildingBlocks.Domain;
using OnlineShop.Modules.Catalog.Domain;

namespace OnlineShop.UnitTests.Catalog;

public class ProductReviewTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Create_WithRatingInRange_Succeeds(int rating)
    {
        var review = ProductReview.Create(Guid.NewGuid(), Guid.NewGuid(), "Alice", rating, "Great product");

        Assert.Equal(rating, review.Rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithRatingOutOfRange_Throws(int rating)
    {
        Assert.Throws<DomainException>(() => ProductReview.Create(Guid.NewGuid(), Guid.NewGuid(), "Alice", rating, null));
    }

    [Fact]
    public void Create_WithNullComment_Succeeds()
    {
        var review = ProductReview.Create(Guid.NewGuid(), Guid.NewGuid(), "Alice", 4, null);

        Assert.Null(review.Comment);
    }

    [Fact]
    public void Create_TrimsBuyerNameAndComment()
    {
        var review = ProductReview.Create(Guid.NewGuid(), Guid.NewGuid(), "  Alice  ", 5, "  Nice!  ");

        Assert.Equal("Alice", review.BuyerName);
        Assert.Equal("Nice!", review.Comment);
    }
}
