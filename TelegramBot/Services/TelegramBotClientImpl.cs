using System.Net.Mime;
using Infrastructure;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Services;

public class TelegramBotClientImpl : ITelegramBot
{
    private readonly ITelegramBotClient _client;
    public TelegramBotClientImpl(ITelegramBotClient client)
    {
        _client = client;
    }
    
    public async Task<IMessage> SendTextMessageAsync(long chatId, string text)
    {
        var message = await _client.SendTextMessageAsync(chatId, text);
        return new TelegramMessage(message);
    }

    public async Task<IMessage> SendTextMessageAsync(long chatId, string text, TelegramKeyboard? keyboard = null, bool useMarkdown = false,
        CancellationToken cancellationToken = default)
    {
        InlineKeyboardMarkup? inlineKeyboard = null;
        if (keyboard != null)
        {
            inlineKeyboard = new InlineKeyboardMarkup(
                keyboard.Buttons.Select(row => 
                    row.Select(button => InlineKeyboardButton.WithCallbackData(button.Text, button.CallbackData)))
            );
        }

        var textToSend = text;
        if (useMarkdown)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(textToSend)}```";
        }

        var message = await _client.SendTextMessageAsync(chatId, textToSend, replyMarkup:inlineKeyboard, 
            parseMode: useMarkdown? ParseMode.MarkdownV2 : null, 
            cancellationToken: cancellationToken);
        return new TelegramMessage(message);
    }

    public async Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default)
    {
        await _client.SetMyCommandsAsync(
            buttons.Select(c => new BotCommand(){Description = c.Text, Command = c.CallbackData}),
            scope: BotCommandScope.Default(), 
            cancellationToken:cancellationToken);
    }

    public async Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken)
    {
        await _client.DeleteMessageAsync(chatId, messageId, cancellationToken);
    }

    public async Task<IFile?> GetFileAsync(string fileId, string? mimeType, CancellationToken cancellationToken)
    {
        if (mimeType != MediaTypeNames.Application.Json)
            return null;
        
        var file = await _client.GetFileAsync(fileId, cancellationToken);

        if (file?.FilePath == null) return null;
        
        string text;
        using (var memoryStream = new MemoryStream())
        {
            await _client.DownloadFileAsync(file.FilePath, memoryStream, cancellationToken);
            var bytes = memoryStream.ToArray();
            text = System.Text.Encoding.Default.GetString(bytes);
        }
        
        return new TelegramFile(){Text = text};
    }

    public async Task SetWebhookAsync(string url)
    {
        await _client.SetWebhookAsync(url);
    }
    
    public async Task<TelegramWebHookInfo> GetWebhookInfoAsync()
    {
        var webHookInfo = await _client.GetWebhookInfoAsync();

        return new TelegramWebHookInfo()
        {
            Url = webHookInfo.Url,
            LastErrorDate = webHookInfo.LastErrorDate,
            LastErrorMessage = webHookInfo.LastErrorMessage,
            PendingUpdateCount = webHookInfo.PendingUpdateCount,
            IpAddress = webHookInfo.IpAddress,
            MaxConnections = webHookInfo.MaxConnections,
        };
    }
}