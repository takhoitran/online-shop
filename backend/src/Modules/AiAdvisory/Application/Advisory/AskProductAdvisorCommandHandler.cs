using System.Text.Json;
using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.AiAdvisory.Application.Abstractions;
using OnlineShop.Modules.AiAdvisory.Domain;
using OnlineShop.Modules.Catalog.Application.Products;

namespace OnlineShop.Modules.AiAdvisory.Application.Advisory;

/// <summary>
/// Buyer-facing product-advisor chat. The model is never allowed to answer product questions
/// from its own "knowledge" — it must call the search_products tool, which runs the exact same
/// GetProductsQuery the Web storefront and Telegram bot use, so it can never invent a product,
/// price, or stock level that doesn't exist. Bounded to 3 tool-call rounds so a misbehaving model
/// can't loop forever burning Gemini quota on one request.
/// </summary>
public sealed class AskProductAdvisorCommandHandler : IRequestHandler<AskProductAdvisorCommand, Result<string>>
{
    private const int MaxRounds = 3;
    private const string FallbackText = "Sorry, I can't answer that right now. Please try again later.";

    private const string SystemInstruction =
        "You are the shopping assistant for the OnlineShop online retail store. " +
        "Always reply in English, keeping it short, friendly, and concise. " +
        "When a customer asks about a product, price, or stock, you MUST call the search_products " +
        "function to get real data — never make up a product name, price, or stock level. " +
        "If no matching product is found in the results, clearly say the store doesn't currently carry it. " +
        "If the question isn't related to shopping at this store, politely decline and invite the customer to ask about products.";

    private static readonly AiFunctionDeclaration SearchProductsTool = new(
        Name: "search_products",
        Description: "Searches for products the store currently sells, by keyword and/or price range. Always use this when the customer asks about a specific product, price, or stock.",
        ParametersJsonSchema: """
        {
          "type": "object",
          "properties": {
            "keyword": { "type": "string", "description": "Search keyword, e.g. a product name or category" },
            "minPrice": { "type": "number", "description": "Minimum price in VND, leave blank if the customer didn't specify one" },
            "maxPrice": { "type": "number", "description": "Maximum price in VND, leave blank if the customer didn't specify one" }
          }
        }
        """);

    private readonly ISender _sender;
    private readonly IGenerativeAiClient _aiClient;
    private readonly IAiInteractionLogRepository _logRepository;
    private readonly IAiAdvisoryUnitOfWork _unitOfWork;

    public AskProductAdvisorCommandHandler(
        ISender sender,
        IGenerativeAiClient aiClient,
        IAiInteractionLogRepository logRepository,
        IAiAdvisoryUnitOfWork unitOfWork)
    {
        _sender = sender;
        _aiClient = aiClient;
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(AskProductAdvisorCommand request, CancellationToken cancellationToken)
    {
        var history = new List<AiChatTurn> { AiChatTurn.FromUser(request.Message) };
        string? finalText = null;

        for (var round = 0; round < MaxRounds && finalText is null; round++)
        {
            var result = await _aiClient.GenerateAsync(SystemInstruction, history, [SearchProductsTool], cancellationToken: cancellationToken);

            if (result.FunctionCall is { Name: "search_products" } call)
            {
                history.Add(AiChatTurn.FromModel(call));
                var responseJson = await ExecuteSearchProductsAsync(call.ArgumentsJson, cancellationToken);
                history.Add(AiChatTurn.FromFunctionResult(call.Name, responseJson));
                continue;
            }

            finalText = result.Text;
        }

        finalText ??= FallbackText;

        _logRepository.Add(AiInteractionLog.Create(request.UserId, request.Channel, request.Message, finalText));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(finalText);
    }

    private async Task<string> ExecuteSearchProductsAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        string? keyword = null;
        decimal? minPrice = null;
        decimal? maxPrice = null;

        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("keyword", out var keywordElement) && keywordElement.ValueKind == JsonValueKind.String)
                keyword = keywordElement.GetString();
            if (root.TryGetProperty("minPrice", out var minElement) && minElement.ValueKind == JsonValueKind.Number)
                minPrice = minElement.GetDecimal();
            if (root.TryGetProperty("maxPrice", out var maxElement) && maxElement.ValueKind == JsonValueKind.Number)
                maxPrice = maxElement.GetDecimal();
        }
        catch (JsonException)
        {
            // Malformed args from the model — search unfiltered rather than failing the whole turn.
        }

        var products = await _sender.Send(new GetProductsQuery(keyword, null, minPrice, maxPrice, 1, 5), cancellationToken);

        var payload = new
        {
            products = products.Items.Select(p => new
            {
                name = p.Name,
                price = p.Price,
                currency = p.Currency,
                stockQuantity = p.StockQuantity,
                category = p.CategoryName
            })
        };

        return JsonSerializer.Serialize(payload);
    }
}
