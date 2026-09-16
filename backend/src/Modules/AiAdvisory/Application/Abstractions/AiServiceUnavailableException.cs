namespace OnlineShop.Modules.AiAdvisory.Application.Abstractions;

/// <summary>Thrown by IGenerativeAiClient implementations when the underlying AI provider itself
/// is rate-limiting or temporarily unavailable (e.g. Gemini's free-tier quota) — distinct from a
/// generic failure so the Api can return a clear 503 instead of a bare 500, and so a future retry
/// policy has something specific to catch.</summary>
public sealed class AiServiceUnavailableException : Exception
{
    public AiServiceUnavailableException(string message) : base(message)
    {
    }
}
