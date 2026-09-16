using System.Net;
using System.Net.Http.Json;

namespace OnlineShop.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class AuthApiTests
{
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string UniqueUsername(string prefix) => $"{prefix}_{Guid.NewGuid():N}"[..20];

    [Fact]
    public async Task Register_WithNewUsername_ReturnsTokenAndBuyerRole()
    {
        var username = UniqueUsername("reguser");

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            password = "Buyer@12345",
            fullName = "Integration Test User"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResultResponse>();

        Assert.NotNull(body);
        Assert.Equal("Buyer", body!.role);
        Assert.False(string.IsNullOrWhiteSpace(body.token));
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        var username = UniqueUsername("dupuser");
        var payload = new { username, password = "Buyer@12345", fullName = "First" };

        (await _client.PostAsJsonAsync("/api/auth/register", payload)).EnsureSuccessStatusCode();
        var second = await _client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsBadRequest()
    {
        var username = UniqueUsername("loginuser");
        await _client.PostAsJsonAsync("/api/auth/register", new { username, password = "Correct@12345", fullName = "Login Test" });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username, password = "Wrong@12345" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        var username = UniqueUsername("loginok");
        await _client.PostAsJsonAsync("/api/auth/register", new { username, password = "Correct@12345", fullName = "Login Ok" });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { username, password = "Correct@12345" });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResultResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body!.token));
    }

    private sealed record AuthResultResponse(Guid userId, string username, string fullName, string role, string token, DateTime expiresAtUtc);
}
