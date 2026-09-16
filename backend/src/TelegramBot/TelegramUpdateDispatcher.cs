using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using OnlineShop.Modules.Catalog.Application.Categories;
using OnlineShop.Modules.Catalog.Application.Products;
using OnlineShop.Modules.Identity.Application.Dtos;
using OnlineShop.Modules.Identity.Application.TelegramLink;
using OnlineShop.Modules.Identity.Application.Users;
using OnlineShop.Modules.Ordering.Application.Carts;
using OnlineShop.Modules.Ordering.Application.Orders;
using OnlineShop.TelegramBot.Rendering;

namespace OnlineShop.TelegramBot;

/// <summary>
/// The Telegram Channel Adapter itself: translates Updates into the exact same Commands/Queries
/// the Web API controllers send — see Ordering.Application.Orders.CheckoutCommand etc. Nothing
/// here duplicates business logic; a bug fixed for Web is automatically fixed for Telegram too.
/// One DI scope per Update (mirrors one scope per HTTP request in the Api).
/// </summary>
public sealed class TelegramUpdateDispatcher
{
    private static readonly Regex LinkCodePattern = new(@"^\d{6}$", RegexOptions.Compiled);

    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>Chat is mid-checkout, waiting for one free-text reply with shipping details.
    /// In-memory is fine — this dispatcher is a singleton and the wait never outlives the process.</summary>
    private readonly Dictionary<long, string> _pendingCheckoutPaymentMethod = new();

    public TelegramUpdateDispatcher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task HandleAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        if (update.Message is { Text: { } text } message)
            await HandleMessageAsync(bot, sender, message, text, cancellationToken);
        else if (update.CallbackQuery is { Data: { } data } callback)
            await HandleCallbackAsync(bot, sender, callback, data, cancellationToken);
    }

    private async Task HandleMessageAsync(ITelegramBotClient bot, ISender sender, Message message, string text, CancellationToken ct)
    {
        var chatId = message.Chat.Id;

        if (text.StartsWith("/start"))
        {
            await bot.SendMessage(chatId, BotText.Welcome, parseMode: ParseMode.Markdown, replyMarkup: BotKeyboards.MainMenu(), cancellationToken: ct);
            return;
        }

        if (LinkCodePattern.IsMatch(text.Trim()))
        {
            var result = await sender.Send(new ConfirmTelegramLinkCommand(text.Trim(), chatId.ToString()), ct);
            var user = result.IsSuccess ? await sender.Send(new GetUserByTelegramIdQuery(chatId.ToString()), ct) : null;
            await bot.SendMessage(
                chatId,
                result.IsSuccess ? BotText.LinkSuccess(user?.FullName ?? "") : $"❌ {result.Error.Message}",
                cancellationToken: ct);
            return;
        }

        if (_pendingCheckoutPaymentMethod.TryGetValue(chatId, out var paymentMethod))
        {
            await HandleShippingAddressReplyAsync(bot, sender, chatId, paymentMethod, text, ct);
            return;
        }

        switch (text)
        {
            case "🛍 Products":
            case "/products":
                await ShowCategoriesAsync(bot, sender, chatId, null, ct);
                return;

            case "🧺 Cart":
            case "/cart":
                await ShowCartAsync(bot, sender, chatId, ct);
                return;

            case "📦 My orders":
            case "/orders":
                await ShowOrdersAsync(bot, sender, chatId, ct);
                return;

            default:
                await bot.SendMessage(chatId, "Type /start for instructions, or use the menu below 👇", replyMarkup: BotKeyboards.MainMenu(), cancellationToken: ct);
                return;
        }
    }

    private async Task HandleCallbackAsync(ITelegramBotClient bot, ISender sender, CallbackQuery callback, string data, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;
        var parts = data.Split(':');

        switch (parts[0])
        {
            case "cats":
                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await ShowCategoriesAsync(bot, sender, chatId, messageId, ct);
                return;

            case "cat":
                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await ShowProductsAsync(bot, sender, chatId, messageId, Guid.Parse(parts[1]), ct);
                return;

            case "prod":
                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await ShowProductDetailAsync(bot, sender, chatId, messageId, Guid.Parse(parts[1]), ct);
                return;

            case "add":
                await HandleAddToCartAsync(bot, sender, callback, Guid.Parse(parts[1]), ct);
                return;

            case "cart":
                await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
                await bot.SendMessage(chatId, "Choose a payment method:", replyMarkup: BotKeyboards.CheckoutChoice(), cancellationToken: ct);
                return;

            case "checkout":
                await HandleCheckoutChosenAsync(bot, sender, callback, parts[1], ct);
                return;
        }
    }

    private static async Task<UserSummaryDto?> ResolveLinkedBuyerAsync(ISender sender, long chatId, ITelegramBotClient bot, CancellationToken ct, bool viaCallback = false, string? callbackId = null)
    {
        var user = await sender.Send(new GetUserByTelegramIdQuery(chatId.ToString()), ct);
        if (user is null)
        {
            if (viaCallback && callbackId is not null) await bot.AnswerCallbackQuery(callbackId, BotText.NotLinked, showAlert: true, cancellationToken: ct);
            else await bot.SendMessage(chatId, BotText.NotLinked, cancellationToken: ct);
            return null;
        }
        if (user.Role != "Buyer")
        {
            if (viaCallback && callbackId is not null) await bot.AnswerCallbackQuery(callbackId, BotText.BuyerOnly, showAlert: true, cancellationToken: ct);
            else await bot.SendMessage(chatId, BotText.BuyerOnly, cancellationToken: ct);
            return null;
        }
        return user;
    }

    private async Task ShowCategoriesAsync(ITelegramBotClient bot, ISender sender, long chatId, int? editMessageId, CancellationToken ct)
    {
        var categories = await sender.Send(new GetCategoriesQuery(), ct);
        const string text = "📂 Choose a product category:";
        var keyboard = BotKeyboards.Categories(categories.Select(c => (c.Id, c.Name)));

        if (editMessageId is { } id)
            await bot.EditMessageText(chatId, id, text, replyMarkup: keyboard, cancellationToken: ct);
        else
            await bot.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
    }

    private async Task ShowProductsAsync(ITelegramBotClient bot, ISender sender, long chatId, int messageId, Guid categoryId, CancellationToken ct)
    {
        var products = await sender.Send(new GetProductsQuery(null, categoryId, null, null, 1, 8), ct);
        if (products.Items.Count == 0)
        {
            await bot.EditMessageText(chatId, messageId, "This category has no products yet.", replyMarkup: BotKeyboards.Products([]), cancellationToken: ct);
            return;
        }

        await bot.EditMessageText(
            chatId, messageId, "🛒 Choose a product:",
            replyMarkup: BotKeyboards.Products(products.Items.Select(p => (p.Id, BotText.ProductLine(p)))),
            cancellationToken: ct);
    }

    private async Task ShowProductDetailAsync(ITelegramBotClient bot, ISender sender, long chatId, int messageId, Guid productId, CancellationToken ct)
    {
        var product = await sender.Send(new GetProductByIdQuery(productId), ct);
        if (product is null)
        {
            await bot.EditMessageText(chatId, messageId, "This product no longer exists.", cancellationToken: ct);
            return;
        }

        await bot.EditMessageText(
            chatId, messageId, BotText.ProductDetail(product),
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.ProductDetail(product.Id),
            cancellationToken: ct);
    }

    private async Task HandleAddToCartAsync(ITelegramBotClient bot, ISender sender, CallbackQuery callback, Guid productId, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var user = await ResolveLinkedBuyerAsync(sender, chatId, bot, ct, viaCallback: true, callbackId: callback.Id);
        if (user is null) return;

        var result = await sender.Send(new AddCartItemCommand(user.Id, productId, 1), ct);
        await bot.AnswerCallbackQuery(callback.Id, result.IsSuccess ? "✅ Added to cart" : $"❌ {result.Error.Message}", showAlert: !result.IsSuccess, cancellationToken: ct);
    }

    private async Task ShowCartAsync(ITelegramBotClient bot, ISender sender, long chatId, CancellationToken ct)
    {
        var user = await ResolveLinkedBuyerAsync(sender, chatId, bot, ct);
        if (user is null) return;

        var cart = await sender.Send(new GetCartQuery(user.Id), ct);
        await bot.SendMessage(
            chatId, BotText.Cart(cart),
            parseMode: ParseMode.Markdown,
            replyMarkup: cart.Items.Count > 0 ? BotKeyboards.CartActions() : null,
            cancellationToken: ct);
    }

    private async Task HandleCheckoutChosenAsync(ITelegramBotClient bot, ISender sender, CallbackQuery callback, string paymentMethod, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var user = await ResolveLinkedBuyerAsync(sender, chatId, bot, ct, viaCallback: true, callbackId: callback.Id);
        if (user is null) return;

        _pendingCheckoutPaymentMethod[chatId] = paymentMethod;
        await bot.AnswerCallbackQuery(callback.Id, cancellationToken: ct);
        await bot.SendMessage(
            chatId,
            "📮 Please reply with your shipping details in *one message*, comma-separated:\n" +
            "`Full name, Phone number, Address, City`\n" +
            "(the Address itself may contain commas — only the very first and very last values are read as name/phone and city)\n\n" +
            "Example: `Nguyen Van A, 0912345678, 12 Nguyen Trai, Thanh Xuan, Hanoi`",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleShippingAddressReplyAsync(ITelegramBotClient bot, ISender sender, long chatId, string paymentMethod, string text, CancellationToken ct)
    {
        // A real street address very often has its own internal comma ("12 Nguyen Trai, Thanh
        // Xuan") — requiring exactly 4 parts would reject every such reply. Instead: the first
        // comma-separated value is always the name, the second the phone, the last is always the
        // city, and everything in between (however many commas that spans) is re-joined back into
        // one Address string.
        var parts = text.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length < 4 || parts.Any(string.IsNullOrWhiteSpace))
        {
            await bot.SendMessage(
                chatId,
                "⚠️ Please send at least 4 comma-separated values: Full name, Phone number, Address, City",
                cancellationToken: ct);
            return;
        }

        var recipientName = parts[0];
        var phoneNumber = parts[1];
        var city = parts[^1];
        var addressLine = string.Join(", ", parts[2..^1]);

        var user = await ResolveLinkedBuyerAsync(sender, chatId, bot, ct);
        if (user is null)
        {
            _pendingCheckoutPaymentMethod.Remove(chatId);
            return;
        }

        var result = await sender.Send(new CheckoutCommand(user.Id, paymentMethod, recipientName, phoneNumber, addressLine, city), ct);
        _pendingCheckoutPaymentMethod.Remove(chatId);

        await bot.SendMessage(
            chatId,
            result.IsSuccess ? BotText.OrderConfirmation(result.Value.ToString()) : $"❌ {result.Error.Message}",
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.MainMenu(),
            cancellationToken: ct);
    }

    private async Task ShowOrdersAsync(ITelegramBotClient bot, ISender sender, long chatId, CancellationToken ct)
    {
        var user = await ResolveLinkedBuyerAsync(sender, chatId, bot, ct);
        if (user is null) return;

        var orders = await sender.Send(new GetOrdersQuery(user.Id, null, 1, 10), ct);
        var text = orders.Items.Count == 0
            ? "You don't have any orders yet."
            : "📦 *Recent orders:*\n\n" + string.Join('\n', orders.Items.Select(BotText.OrderLine));

        await bot.SendMessage(chatId, text, parseMode: ParseMode.Markdown, cancellationToken: ct);
    }
}
