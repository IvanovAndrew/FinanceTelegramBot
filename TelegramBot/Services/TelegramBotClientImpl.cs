using System.Net.Mime;
using Infrastructure;
using Infrastructure.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Services;

public class TelegramBotLogDecorator : ITelegramBot
{
    private readonly ITelegramBot _source;
    private readonly ILogger _logger;

    public TelegramBotLogDecorator(ITelegramBot source, ILogger logger)
    {
        _source = source;
        _logger = logger;
    }
    
    public Task<IMessage> SendTextMessageAsync(long chatId, string text)
    {
        _logger.LogInformation(text);
        return _source.SendTextMessageAsync(chatId, text);
    }

    public Task<IMessage> SendTextMessageAsync(long chatId, string? text = null, TelegramKeyboard? keyboard = null, bool useMarkdown = false,
        CancellationToken cancellationToken = default)
    {
        return _source.SendTextMessageAsync(chatId, text, keyboard, useMarkdown, cancellationToken);
    }

    public Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default)
    {
        return _source.SetMyCommandsAsync(buttons, cancellationToken);
    }

    public async Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Removing message {message.Id} \"{message.Text}\"");
            await _source.DeleteMessageAsync(message, cancellationToken);
            _logger.LogInformation($"Message {message.Id} \"{message.Text}\" is removed");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }

    public Task<IFile?> GetFileAsync(string fileId, string? mimeType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Telegram: get file");
        return _source.GetFileAsync(fileId, mimeType, cancellationToken);
    }

    public Task<TelegramWebHookInfo> GetWebhookInfoAsync()
    {
        _logger.LogInformation($"Telegram: get web hook info");
        return _source.GetWebhookInfoAsync();
    }

    public Task SetWebhookAsync(string url)
    {
        _logger.LogInformation($"Telegram: set web hook to {url}");
        return _source.SetWebhookAsync(url);
    }
}

public class TelegramBotClientImpl : ITelegramBot
{
    private readonly ITelegramBotClient _client;
    private readonly IDateTimeService _dateTimeService;

    public TelegramBotClientImpl(ITelegramBotClient client, IDateTimeService dateTimeService)
    {
        _client = client;
        _dateTimeService = dateTimeService;
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