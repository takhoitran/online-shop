using System.Globalization;
using System.Text;
using OnlineShop.Modules.Catalog.Application.Dtos;
using OnlineShop.Modules.Ordering.Application.Dtos;

namespace OnlineShop.TelegramBot.Rendering;

internal static class BotText
{
    private static string Money(decimal amount, string currency) =>
        currency == "VND" ? $"{amount.ToString("N0", CultureInfo.InvariantCulture)}₫" : $"{amount:N0} {currency}";

    /// <summary>Every message here is sent with `parseMode: Markdown` — any free text that isn't
    /// hard-coded by us (a product name/description an Admin or Seller typed in) must be escaped
    /// before it's interpolated in, or a stray `_`/`*`/`` ` ``/`[` breaks the message's formatting
    /// and can make Telegram's API reject the whole send with "can't parse entities".</summary>
    private static string Escape(string? text) =>
        string.IsNullOrEmpty(text)
            ? string.Empty
            : text.Replace("\\", "\\\\").Replace("_", "\\_").Replace("*", "\\*").Replace("`", "\\`").Replace("[", "\\[");

    public const string Welcome =
        "👋 Welcome to *OnlineShop*!\n\n" +
        "You can browse products right away without logging in.\n" +
        "To add items to your cart & place orders, link your Buyer account:\n" +
        "1️⃣ Log in on the Web\n" +
        "2️⃣ Go to the Telegram link section to get a 6-digit code\n" +
        "3️⃣ Send that code to me here\n\n" +
        "Use the menu below to get started 👇";

    public const string NotLinked =
        "🔒 This Telegram account isn't linked to a Buyer account yet.\n" +
        "Log in on the Web, get a 6-digit link code, then send it to me here.";

    public const string BuyerOnly = "⚠️ This feature is only available to Buyer accounts.";

    public static string LinkSuccess(string fullName) => $"✅ Linked successfully! Hello, {fullName}.";

    public static string ProductLine(ProductListItemDto p) =>
        $"{p.Name} — {Money(p.Price, p.Currency)}";

    public static string ProductDetail(ProductDetailDto p) =>
        $"*{Escape(p.Name)}*\n{Money(p.Price, p.Currency)}\n" +
        $"Stock: {(p.StockQuantity > 0 ? $"{p.StockQuantity} available" : "out of stock")}\n\n" +
        (string.IsNullOrWhiteSpace(p.Description) ? "" : Escape(p.Description));

    public static string Cart(CartDto cart)
    {
        if (cart.Items.Count == 0)
            return "🧺 Your cart is empty. Tap \"🛍 Products\" to start shopping.";

        var sb = new StringBuilder("🧺 *Your cart:*\n\n");
        foreach (var item in cart.Items)
            sb.AppendLine($"• {Escape(item.ProductName)} x{item.Quantity} = {Money(item.LineTotal, item.Currency)}");
        sb.AppendLine();
        sb.Append($"*Total: {Money(cart.Total, cart.Currency)}*");
        return sb.ToString();
    }

    public static string OrderConfirmation(string orderId) =>
        $"✅ Order placed successfully!\nOrder code: `{orderId[..8].ToUpperInvariant()}`\n" +
        "Your order is awaiting Seller approval. You'll be notified of any updates.";

    public static string OrderLine(OrderSummaryDto o) =>
        $"#{o.Id.ToString()[..8].ToUpperInvariant()} — {StatusLabel(o.Status)} — {Money(o.TotalAmount, o.Currency)}";

    public static string StatusLabel(string status) => status switch
    {
        "Pending" => "Pending",
        "Approved" => "Approved",
        "Shipping" => "Shipping",
        "Completed" => "Completed",
        "Cancelled" => "Cancelled",
        "Returned" => "Returned",
        _ => status,
    };
}
