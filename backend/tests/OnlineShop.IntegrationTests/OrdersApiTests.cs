using System.Net;
using System.Net.Http.Json;

namespace OnlineShop.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class OrdersApiTests
{
    private readonly HttpClient _client;

    public OrdersApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterBuyerAsync(string prefix)
    {
        var username = $"{prefix}_{Guid.NewGuid():N}"[..20];
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            password = "Buyer@12345",
            fullName = prefix
        });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResultResponse>();
        return auth!.token;
    }

    private static HttpRequestMessage AuthedRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Authorization", $"Bearer {token}");
        return request;
    }

    [Fact]
    public async Task Checkout_WithEmptyCart_ReturnsBadRequest()
    {
        var token = await RegisterBuyerAsync("emptycart");

        using var request = AuthedRequest(HttpMethod.Post, "/api/orders/checkout", token);
        request.Content = JsonContent.Create(new { paymentMethod = "Cod" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithNonexistentProduct_ReturnsBadRequest()
    {
        var token = await RegisterBuyerAsync("badcart");

        using var request = AuthedRequest(HttpMethod.Post, "/api/cart/items", token);
        request.Content = JsonContent.Create(new { productId = Guid.NewGuid(), quantity = 1 });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderById_AsDifferentBuyer_ReturnsForbidden()
    {
        var ownerToken = await RegisterBuyerAsync("orderowner");
        var otherToken = await RegisterBuyerAsync("otherbuyer");

        var productId = await GetAnyProductIdAsync();

        using (var addRequest = AuthedRequest(HttpMethod.Post, "/api/cart/items", ownerToken))
        {
            addRequest.Content = JsonContent.Create(new { productId, quantity = 1 });
            (await _client.SendAsync(addRequest)).EnsureSuccessStatusCode();
        }

        using var checkoutRequest = AuthedRequest(HttpMethod.Post, "/api/orders/checkout", ownerToken);
        checkoutRequest.Content = JsonContent.Create(new
        {
            paymentMethod = "Cod",
            recipientName = "Order Owner",
            phoneNumber = "0900000000",
            addressLine = "1 Test Street",
            city = "Test City"
        });
        var checkoutResponse = await _client.SendAsync(checkoutRequest);
        checkoutResponse.EnsureSuccessStatusCode();
        var orderId = (await checkoutResponse.Content.ReadFromJsonAsync<Guid>());

        using var getRequest = AuthedRequest(HttpMethod.Get, $"/api/orders/{orderId}", otherToken);
        var getResponse = await _client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);
    }

    /// <summary>The test database starts empty (only IdentitySeeder's default admin account
    /// exists) — unlike the dev database, there's no guarantee any product already exists, so
    /// tests that need one create it themselves via the real Admin endpoints.</summary>
    private async Task<Guid> GetAnyProductIdAsync()
    {
        var adminLogin = await _client.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "Admin@123" });
        adminLogin.EnsureSuccessStatusCode();
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<AuthResultResponse>())!.token;

        using var createCategory = AuthedRequest(HttpMethod.Post, "/api/categories", adminToken);
        createCategory.Content = JsonContent.Create(new { name = $"Test Category {Guid.NewGuid():N}"[..30], description = (string?)null });
        var categoryResponse = await _client.SendAsync(createCategory);
        categoryResponse.EnsureSuccessStatusCode();
        var categoryId = (await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>())!.id;

        using var createProduct = AuthedRequest(HttpMethod.Post, "/api/products", adminToken);
        createProduct.Content = JsonContent.Create(new { name = "Integration Test Product", description = (string?)null, price = 10_000, categoryId });
        var productResponse = await _client.SendAsync(createProduct);
        productResponse.EnsureSuccessStatusCode();
        var productId = await productResponse.Content.ReadFromJsonAsync<Guid>();

        // A freshly created product starts at 0 stock — checkout's stock-availability check
        // (added alongside this test file) would otherwise reject every test that adds it to a
        // cart, the same way a real empty-stock product would.
        using var receiveStock = AuthedRequest(HttpMethod.Post, "/api/inventory/receive", adminToken);
        receiveStock.Content = JsonContent.Create(new { productId, quantity = 100, note = "Integration test stock" });
        (await _client.SendAsync(receiveStock)).EnsureSuccessStatusCode();

        return productId;
    }

    private sealed record AuthResultResponse(Guid userId, string username, string fullName, string role, string token, DateTime expiresAtUtc);
    private sealed record CategoryResponse(Guid id, string name, string? description);
}
