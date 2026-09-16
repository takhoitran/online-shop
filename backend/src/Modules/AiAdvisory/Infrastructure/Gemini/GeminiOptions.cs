namespace OnlineShop.Modules.AiAdvisory.Infrastructure.Gemini;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Configurable so switching models (e.g. for cost or quality) never touches code.</summary>
    public string Model { get; set; } = "gemini-3.6-flash";
}
