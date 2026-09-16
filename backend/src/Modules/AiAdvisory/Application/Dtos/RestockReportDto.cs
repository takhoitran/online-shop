namespace OnlineShop.Modules.AiAdvisory.Application.Dtos;

public sealed record RestockSuggestionDto(string ProductName, string Action, string Reason);

public sealed record RestockReportDto(string Summary, IReadOnlyList<RestockSuggestionDto> Suggestions);
