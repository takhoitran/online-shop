using Telegram.Bot.Types.ReplyMarkups;

namespace OnlineShop.TelegramBot.Rendering;

/// <summary>Callback data stays under Telegram's 64-byte limit by design — short prefixes, raw
/// Guids with no separators added beyond the single ':' delimiter.</summary>
internal static class BotKeyboards
{
    public static ReplyKeyboardMarkup MainMenu() => new(
    [
        [new KeyboardButton("🛍 Products"), new KeyboardButton("🧺 Cart")],
        [new KeyboardButton("📦 My orders")],
    ])
    { ResizeKeyboard = true };

    public static InlineKeyboardMarkup Categories(IEnumerable<(Guid Id, string Name)> categories) =>
        new(categories.Select(c => new[] { InlineKeyboardButton.WithCallbackData(c.Name, $"cat:{c.Id}") }));

    public static InlineKeyboardMarkup Products(IEnumerable<(Guid Id, string Label)> products)
    {
        var rows = products.Select(p => new[] { InlineKeyboardButton.WithCallbackData(p.Label, $"prod:{p.Id}") }).ToList();
        rows.Add([InlineKeyboardButton.WithCallbackData("◀ Categories", "cats")]);
        return new InlineKeyboardMarkup(rows);
    }

    public static InlineKeyboardMarkup ProductDetail(Guid productId) => new(
    [
        [InlineKeyboardButton.WithCallbackData("➕ Add to cart", $"add:{productId}")],
        [InlineKeyboardButton.WithCallbackData("◀ Categories", "cats")],
    ]);

    public static InlineKeyboardMarkup CheckoutChoice() => new(
    [
        [InlineKeyboardButton.WithCallbackData("💵 COD (pay on delivery)", "checkout:Cod")],
        [InlineKeyboardButton.WithCallbackData("🏦 Bank transfer", "checkout:BankTransfer")],
    ]);

    public static InlineKeyboardMarkup CartActions() => new(
    [
        [InlineKeyboardButton.WithCallbackData("✅ Place order", "cart:checkout")],
    ]);
}
