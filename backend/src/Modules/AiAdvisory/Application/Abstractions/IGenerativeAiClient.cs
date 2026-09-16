namespace OnlineShop.Modules.AiAdvisory.Application.Abstractions;

public enum AiChatRole
{
    User,
    Model,
    Function
}

/// <summary>One turn in a generative-AI conversation. Exactly one of Text/FunctionCall/
/// FunctionResponseJson is set, matching which Role it carries.</summary>
public sealed record AiChatTurn
{
    public required AiChatRole Role { get; init; }
    public string? Text { get; init; }
    public AiFunctionCall? FunctionCall { get; init; }
    public string? FunctionName { get; init; }
    public string? FunctionResponseJson { get; init; }

    public static AiChatTurn FromUser(string text) => new() { Role = AiChatRole.User, Text = text };
    public static AiChatTurn FromModel(AiFunctionCall call) => new() { Role = AiChatRole.Model, FunctionCall = call };
    public static AiChatTurn FromFunctionResult(string functionName, string responseJson) =>
        new() { Role = AiChatRole.Function, FunctionName = functionName, FunctionResponseJson = responseJson };
}

/// <summary>A tool the model may call mid-conversation instead of answering directly.
/// ParametersJsonSchema is a plain JSON Schema object (the vendor adapter translates it to
/// whatever wire format that provider expects) — kept here so Application can declare tools
/// without knowing it's talking to Gemini specifically.</summary>
public sealed record AiFunctionDeclaration(string Name, string Description, string ParametersJsonSchema);

/// <summary>ProviderOpaqueToken is round-tripped verbatim (Application never inspects it) — some
/// providers (e.g. Gemini's "thought signature") require their own function-call turn echoed back
/// with this token attached on the next request, or they reject the call.</summary>
public sealed record AiFunctionCall(string Name, string ArgumentsJson, string? ProviderOpaqueToken = null);

public sealed record AiGenerationResult(string? Text, AiFunctionCall? FunctionCall);

/// <summary>Port for "ask a generative AI model something, optionally letting it call back into
/// our own tools". Implemented in Infrastructure against Gemini today; swapping providers later
/// only touches that one adapter, per the Context Map's "AI Advisory calls out via an adapter,
/// never lets the vendor SDK leak into Application" rule.</summary>
public interface IGenerativeAiClient
{
    Task<AiGenerationResult> GenerateAsync(
        string systemInstruction,
        IReadOnlyList<AiChatTurn> history,
        IReadOnlyList<AiFunctionDeclaration>? tools = null,
        bool requireJsonOutput = false,
        CancellationToken cancellationToken = default);
}
