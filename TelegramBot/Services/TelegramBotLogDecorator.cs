using Infrastructure.Telegram;

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
    
    public Task<IMessage> SendTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default)
    {
        return _source.SendTextMessageAsync(messageToSend, cancellationToken);
    }

    public Task<IMessage> EditSentTextMessageAsync(IMessageToSend editedMessage, CancellationToken cancellationToken = default)
    {
        return _source.EditSentTextMessageAsync(editedMessage, cancellationToken);
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