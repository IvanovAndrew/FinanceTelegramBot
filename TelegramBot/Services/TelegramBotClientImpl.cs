using System.Net.Mime;
using Infrastructure;
using Infrastructure.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Services;

public class TelegramBotClientImpl : ITelegramBot
{
    private readonly ITelegramBotClient _client;
    private readonly IDateTimeService _dateTimeService;

    public TelegramBotClientImpl(ITelegramBotClient client, IDateTimeService dateTimeService)
    {
        _client = client;
        _dateTimeService = dateTimeService;
    }
    
    public async Task<IMessage> SendTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default)
    {
        InlineKeyboardMarkup? inlineKeyboard = null;
        if (messageToSend.Keyboard != null)
        {
            inlineKeyboard = new InlineKeyboardMarkup(
                messageToSend.Keyboard.Buttons.Select(row => 
                    row.Select(button => InlineKeyboardButton.WithCallbackData(button.Text, button.CallbackData)))
            );
        }

        var textToSend = messageToSend.Text;
        if (messageToSend.UseMarkdown)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(textToSend)}```";
        }

        var message = await _client.SendTextMessageAsync(messageToSend.ChatId, textToSend, replyMarkup:inlineKeyboard, 
            parseMode: messageToSend.UseMarkdown? ParseMode.MarkdownV2 : null, 
            cancellationToken: cancellationToken);
        return new TelegramMessage(message);
    }

    public async Task<IMessage> EditSentTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default)
    {
        InlineKeyboardMarkup? inlineKeyboard = null;
        if (messageToSend.Keyboard != null)
        {
            inlineKeyboard = new InlineKeyboardMarkup(
                messageToSend.Keyboard.Buttons.Select(row => 
                    row.Select(button => InlineKeyboardButton.WithCallbackData(button.Text, button.CallbackData)))
            );
        }
        
        var message = await _client.EditMessageTextAsync(messageToSend.ChatId, messageToSend.MessageId, messageToSend.Text,
            replyMarkup:inlineKeyboard,
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

    public async Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        var diff = _dateTimeService.Now().Subtract(message.Date);
        if (diff.Hours >= 48)
        {
            throw new DeleteOutdatedTelegramMessageException();
        }

        try
        {
            await _client.DeleteMessageAsync(message.ChatId, message.Id, cancellationToken);
        }
        catch (Exception e)
        {
            throw new TelegramBotException(e);
        }
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

public class TelegramBotException : Exception
{
    public TelegramBotException()
    {
        
    }
    
    public TelegramBotException(Exception exception) : base("Telegram client exception", exception)
    {
    }
}

public class TelegramBotSpecificException : TelegramBotException
{
    public TelegramBotSpecificException(Exception ex) : base(ex)
    {
    }
}

public class DeleteOutdatedTelegramMessageException : TelegramBotException
{
    public override string Message { get; } = "A message can only be deleted if it was sent less than 48 hours ago";
}