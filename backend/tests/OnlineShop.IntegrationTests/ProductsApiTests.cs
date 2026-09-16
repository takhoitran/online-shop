using System.Net;
using System.Net.Http.Json;

namespace OnlineShop.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class ProductsApiTests
{
    private readonly HttpClient _client;

    public ProductsApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_Anonymous_ReturnsPagedResult()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedResultResponse>();

        Assert.NotNull(body);
        Assert.True(body!.pageSize == 5);
    }

    [Fact]
    public async Task Create_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Should Not Be Created",
            description = (string?)null,
            price = 10_000,
            categoryId = Guid.NewGuid()
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsBuyer_ReturnsForbidden()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"buyertest_{Guid.NewGuid():N}"[..20],
            password = "Buyer@12345",
            fullName = "Buyer Trying Admin Action"
        });
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResultResponse>();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
        {
            Content = JsonContent.Create(new { name = "x", description = (string?)null, price = 1000, categoryId = Guid.NewGuid() })
        };
        request.Headers.Add("Authorization", $"Bearer {auth!.token}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record PagedResultResponse(int page, int pageSize, int totalCount, int totalPages);
    private sealed record AuthResultResponse(Guid userId, string username, string fullName, string role, string token, DateTime expiresAtUtc);
}
