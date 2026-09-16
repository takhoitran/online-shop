using System.Text.Json;
using MediatR;
using OnlineShop.BuildingBlocks.Application;
using OnlineShop.Modules.AiAdvisory.Application.Abstractions;
using OnlineShop.Modules.AiAdvisory.Application.Dtos;
using OnlineShop.Modules.AiAdvisory.Domain;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Ordering.Application.Orders;

namespace OnlineShop.Modules.AiAdvisory.Application.Advisory;

/// <summary>
/// Seller-facing restock advisor: pulls current stock from Catalog and confirmed-sale demand
/// from Ordering (both via ISender — AiAdvisory never touches either module's DbContext, per the
/// Context Map), hands the combined numbers to Gemini as data (not as something to search for),
/// and asks for a short natural-language summary plus a small structured suggestion list.
/// </summary>
public sealed class GenerateRestockReportCommandHandler : IRequestHandler<GenerateRestockReportCommand, Result<RestockReportDto>>
{
    private const string SystemInstruction =
        "You are a retail analytics expert advising the owner of an online retail store on restocking. " +
        "You will receive a JSON list of products, each with its current stock on hand (stockQuantity), " +
        "quantity sold from confirmed orders (quantitySold), and the corresponding revenue (revenue). " +
        "Analyze the data and reply with ONLY a single JSON object matching this exact structure, with no " +
        "text outside of that JSON: " +
        """{"summary": "a short summary in English of the overall situation", "suggestions": [{"productName": "...", "action": "Restock or Clearance sale or Keep monitoring", "reason": "a short reason"}]}""" +
        " Give at most 8 suggestions, prioritizing products that sell well but have low stock (need restocking) " +
        "and products with high stock but slow or no sales (need a clearance sale or closer monitoring).";

    private readonly ISender _sender;
    private readonly IGenerativeAiClient _aiClient;
    private readonly IAiInteractionLogRepository _logRepository;
    private readonly IAiAdvisoryUnitOfWork _unitOfWork;

    public GenerateRestockReportCommandHandler(
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

    public async Task<Result<RestockReportDto>> Handle(GenerateRestockReportCommand request, CancellationToken cancellationToken)
    {
        var products = await _sender.Send(new GetProductsQuery(null, null, null, null, 1, 100), cancellationToken);

        if (products.Items.Count == 0)
            return Result.Success(new RestockReportDto("There are no products in the system to analyze yet.", []));

        var salesStats = await _sender.Send(new GetProductSalesStatsQuery(50), cancellationToken);
        var salesByProductId = salesStats.ToDictionary(s => s.ProductId);

        var dataItems = products.Items.Select(p =>
        {
            salesByProductId.TryGetValue(p.Id, out var stats);
            return new RestockSignal(
                p.Name,
                p.StockQuantity,
                stats?.TotalQuantitySold ?? 0,
                stats?.TotalRevenue ?? 0m);
        }).ToList();

        var userMessage = JsonSerializer.Serialize(new { products = dataItems });
        var history = new List<AiChatTurn> { AiChatTurn.FromUser(userMessage) };

        RestockReportDto report;
        string logText;
        try
        {
            var result = await _aiClient.GenerateAsync(
                SystemInstruction,
                history,
                tools: null,
                requireJsonOutput: true,
                cancellationToken: cancellationToken);
            report = ParseReport(result.Text);
            logText = result.Text ?? string.Empty;
        }
        catch (Exception ex) when (IsAiProviderUnavailable(ex))
        {
            report = BuildFallbackReport(dataItems, ex.Message);
            logText = JsonSerializer.Serialize(new
            {
                fallback = true,
                reason = ex.Message,
                report
            });
        }

        _logRepository.Add(AiInteractionLog.Create(request.SellerUserId, "Web", "GenerateRestockReport", logText));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(report);
    }

    private static bool IsAiProviderUnavailable(Exception ex) =>
        ex is AiServiceUnavailableException ||
        ex is TaskCanceledException ||
        ex is HttpRequestException ||
        ex is InvalidOperationException { Message: "Gemini:ApiKey has not been configured." };

    private static RestockReportDto BuildFallbackReport(IReadOnlyList<RestockSignal> products, string fallbackReason)
    {
        var totalProducts = products.Count;
        var outOfStock = products.Count(p => p.StockQuantity == 0);
        var lowStock = products.Count(p => p.StockQuantity is > 0 and <= 10);
        var noSalesHighStock = products.Count(p => p.QuantitySold == 0 && p.StockQuantity >= 25);

        var suggestions = products
            .Select(p => new
            {
                Signal = p,
                Suggestion = BuildFallbackSuggestion(p),
                Priority = CalculatePriority(p)
            })
            .Where(x => x.Suggestion is not null)
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.Signal.Revenue)
            .Take(8)
            .Select(x => x.Suggestion!)
            .ToList();

        var summary =
            $"Local restock report generated from current stock and confirmed sales because Gemini was unavailable: {fallbackReason} " +
            $"Analyzed {totalProducts} products: {outOfStock} out of stock, {lowStock} low-stock, and {noSalesHighStock} high-stock items with no confirmed sales.";

        return new RestockReportDto(summary, suggestions);
    }

    private static RestockSuggestionDto? BuildFallbackSuggestion(RestockSignal product)
    {
        if (product.StockQuantity == 0 && product.QuantitySold > 0)
        {
            return new RestockSuggestionDto(
                product.ProductName,
                "Restock",
                $"Sold {product.QuantitySold} units but currently has no stock.");
        }

        if (product.QuantitySold >= 5 && product.StockQuantity <= 10)
        {
            return new RestockSuggestionDto(
                product.ProductName,
                "Restock",
                $"Strong confirmed sales ({product.QuantitySold} units) with only {product.StockQuantity} left.");
        }

        if (product.QuantitySold >= 3 && product.StockQuantity <= 20)
        {
            return new RestockSuggestionDto(
                product.ProductName,
                "Keep monitoring",
                $"Demand is visible ({product.QuantitySold} units sold) and stock is moderate at {product.StockQuantity}.");
        }

        if (product.QuantitySold == 0 && product.StockQuantity >= 25)
        {
            return new RestockSuggestionDto(
                product.ProductName,
                "Clearance sale",
                $"No confirmed sales yet while stock is high at {product.StockQuantity} units.");
        }

        if (product.QuantitySold <= 2 && product.StockQuantity >= 40)
        {
            return new RestockSuggestionDto(
                product.ProductName,
                "Clearance sale",
                $"Slow movement ({product.QuantitySold} units sold) with high stock on hand.");
        }

        return null;
    }

    private static int CalculatePriority(RestockSignal product)
    {
        if (product.StockQuantity == 0 && product.QuantitySold > 0)
            return 100 + product.QuantitySold;
        if (product.QuantitySold >= 5 && product.StockQuantity <= 10)
            return 80 + product.QuantitySold - product.StockQuantity;
        if (product.QuantitySold == 0 && product.StockQuantity >= 25)
            return 60 + product.StockQuantity;
        if (product.QuantitySold <= 2 && product.StockQuantity >= 40)
            return 50 + product.StockQuantity;
        if (product.QuantitySold >= 3 && product.StockQuantity <= 20)
            return 40 + product.QuantitySold;
        return 0;
    }

    private static RestockReportDto ParseReport(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new RestockReportDto("The AI could not generate a report right now, please try again later.", []);

        try
        {
            using var doc = JsonDocument.Parse(rawText);
            var root = doc.RootElement;
            var summary = root.TryGetProperty("summary", out var summaryElement) ? summaryElement.GetString() ?? string.Empty : string.Empty;

            var suggestions = new List<RestockSuggestionDto>();
            if (root.TryGetProperty("suggestions", out var suggestionsElement) && suggestionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in suggestionsElement.EnumerateArray())
                {
                    var name = item.TryGetProperty("productName", out var n) ? n.GetString() ?? string.Empty : string.Empty;
                    var action = item.TryGetProperty("action", out var a) ? a.GetString() ?? string.Empty : string.Empty;
                    var reason = item.TryGetProperty("reason", out var r) ? r.GetString() ?? string.Empty : string.Empty;
                    if (name.Length > 0)
                        suggestions.Add(new RestockSuggestionDto(name, action, reason));
                }
            }

            return new RestockReportDto(string.IsNullOrWhiteSpace(summary) ? rawText : summary, suggestions);
        }
        catch (JsonException)
        {
            // Model didn't return clean JSON despite the instruction — show the raw text rather than failing the request.
            return new RestockReportDto(rawText, []);
        }
    }

    private sealed record RestockSignal(
        string ProductName,
        int StockQuantity,
        int QuantitySold,
        decimal Revenue);
}
