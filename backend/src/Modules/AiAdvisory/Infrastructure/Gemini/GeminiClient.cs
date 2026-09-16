using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Modules.AiAdvisory.Application.Abstractions;

namespace OnlineShop.Modules.AiAdvisory.Infrastructure.Gemini;

/// <summary>
/// Adapter implementing the vendor-agnostic IGenerativeAiClient port against Google's Gemini
/// "generateContent" REST API (v1beta). All Gemini-specific wire format details — camelCase
/// field names, the "function" role for tool results, and the fact that Gemini's Schema proto
/// wants UPPERCASE type names (OBJECT/STRING/NUMBER) where standard JSON Schema uses lowercase —
/// are translated here so nothing above Application ever needs to know which vendor this is.
/// </summary>
internal sealed class GeminiClient : IGenerativeAiClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiGenerationResult> GenerateAsync(
        string systemInstruction,
        IReadOnlyList<AiChatTurn> history,
        IReadOnlyList<AiFunctionDeclaration>? tools = null,
        bool requireJsonOutput = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("Gemini:ApiKey has not been configured.");

        var requestBody = BuildRequestBody(systemInstruction, history, tools, requireJsonOutput);
        var requestUrl = $"v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

        using var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API returned an error {StatusCode}: {Body}", response.StatusCode, responseBody);

            if (response.StatusCode is System.Net.HttpStatusCode.TooManyRequests or System.Net.HttpStatusCode.ServiceUnavailable)
                throw new AiServiceUnavailableException("The AI service is temporarily busy (rate-limited by the provider). Please try again shortly.");

            throw new InvalidOperationException($"Gemini API call failed with status {response.StatusCode}.");
        }

        return ParseResponse(responseBody);
    }

    private static JsonObject BuildRequestBody(
        string systemInstruction,
        IReadOnlyList<AiChatTurn> history,
        IReadOnlyList<AiFunctionDeclaration>? tools,
        bool requireJsonOutput)
    {
        var contents = new JsonArray();
        foreach (var turn in history)
            contents.Add(BuildContentNode(turn));

        var body = new JsonObject
        {
            ["systemInstruction"] = new JsonObject
            {
                ["parts"] = new JsonArray { new JsonObject { ["text"] = systemInstruction } }
            },
            ["contents"] = contents
        };

        if (tools is { Count: > 0 })
        {
            var functionDeclarations = new JsonArray();
            foreach (var tool in tools)
            {
                var parametersNode = JsonNode.Parse(tool.ParametersJsonSchema);
                UppercaseSchemaTypesInPlace(parametersNode);

                functionDeclarations.Add(new JsonObject
                {
                    ["name"] = tool.Name,
                    ["description"] = tool.Description,
                    ["parameters"] = parametersNode
                });
            }

            body["tools"] = new JsonArray { new JsonObject { ["functionDeclarations"] = functionDeclarations } };
        }

        if (requireJsonOutput)
            body["generationConfig"] = new JsonObject { ["responseMimeType"] = "application/json" };

        return body;
    }

    private static JsonObject BuildContentNode(AiChatTurn turn)
    {
        var role = turn.Role switch
        {
            AiChatRole.User => "user",
            AiChatRole.Model => "model",
            // This model generation rejects a dedicated "function" role ("Role 'function' is not
            // supported" — confirmed against the live API, our docs lookup was stale) — send the
            // functionResponse part as a "user" turn instead, which every Gemini generation accepts.
            AiChatRole.Function => "user",
            _ => "user"
        };

        JsonObject part;
        if (turn.FunctionCall is { } call)
        {
            part = new JsonObject
            {
                ["functionCall"] = new JsonObject
                {
                    ["name"] = call.Name,
                    ["args"] = JsonNode.Parse(string.IsNullOrWhiteSpace(call.ArgumentsJson) ? "{}" : call.ArgumentsJson)
                }
            };

            // Gemini 3's "thinking" models reject a replayed functionCall turn that doesn't carry
            // back the thought signature they issued it with — see
            // https://ai.google.dev/gemini-api/docs/thinking#signatures.
            if (!string.IsNullOrEmpty(call.ProviderOpaqueToken))
                part["thoughtSignature"] = call.ProviderOpaqueToken;
        }
        else if (turn.FunctionResponseJson is not null)
        {
            part = new JsonObject
            {
                ["functionResponse"] = new JsonObject
                {
                    ["name"] = turn.FunctionName,
                    ["response"] = JsonNode.Parse(turn.FunctionResponseJson)
                }
            };
        }
        else
        {
            part = new JsonObject { ["text"] = turn.Text ?? string.Empty };
        }

        return new JsonObject { ["role"] = role, ["parts"] = new JsonArray { part } };
    }

    /// <summary>Recursively uppercases every JSON Schema "type" value (object -> OBJECT, string ->
    /// STRING, ...) to match Gemini's Schema proto enum, mutating the already-detached tree in place.</summary>
    private static void UppercaseSchemaTypesInPlace(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                if (obj.TryGetPropertyValue("type", out var typeNode) &&
                    typeNode is JsonValue typeValue &&
                    typeValue.TryGetValue<string>(out var typeStr))
                {
                    obj["type"] = typeStr.ToUpperInvariant();
                }

                foreach (var property in obj.ToList())
                {
                    if (property.Key != "type")
                        UppercaseSchemaTypesInPlace(property.Value);
                }
                break;

            case JsonArray arr:
                foreach (var item in arr)
                    UppercaseSchemaTypesInPlace(item);
                break;
        }
    }

    private static AiGenerationResult ParseResponse(string responseBody)
    {
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            return new AiGenerationResult(null, null);

        var content = candidates[0].GetProperty("content");
        if (!content.TryGetProperty("parts", out var parts))
            return new AiGenerationResult(null, null);

        string? text = null;
        AiFunctionCall? functionCall = null;

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("functionCall", out var functionCallElement))
            {
                var name = functionCallElement.GetProperty("name").GetString()!;
                var argsJson = functionCallElement.TryGetProperty("args", out var argsElement) ? argsElement.GetRawText() : "{}";
                var thoughtSignature = part.TryGetProperty("thoughtSignature", out var sigElement) ? sigElement.GetString() : null;
                functionCall = new AiFunctionCall(name, argsJson, thoughtSignature);
            }
            else if (part.TryGetProperty("text", out var textElement))
            {
                text = (text ?? string.Empty) + textElement.GetString();
            }
        }

        return new AiGenerationResult(text, functionCall);
    }
}
