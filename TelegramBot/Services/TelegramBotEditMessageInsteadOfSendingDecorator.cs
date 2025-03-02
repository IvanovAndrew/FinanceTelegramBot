using System.Collections.Concurrent;
using Infrastructure.Telegram;

namespace TelegramBot.Services;

internal class TelegramBotEditMessageInsteadOfSendingDecorator : ITelegramBot
{
    private readonly ITelegramBot _source;

    private static ConcurrentDictionary<long, TelegramResponse> _chatToLastSentMessage = new();

    public TelegramBotEditMessageInsteadOfSendingDecorator(ITelegramBot source)
    {
        _source = source;
    }
    
    public async Task<IMessage> SendTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default)
    {
        IMessage message;
        if (_chatToLastSentMessage.TryGetValue(messageToSend.ChatId, out var lastSentMessage) && lastSentMessage.Request.IsEditable)
        {
            messageToSend.MessageId = lastSentMessage.Response.Id;
            try
            {
                message = await _source.EditSentTextMessageAsync(messageToSend, cancellationToken);
            }
            catch (Exception e)
            {
                message = await _source.SendTextMessageAsync(messageToSend, cancellationToken);
                messageToSend.MessageId = message.Id;
            }
        }
        else if (lastSentMessage?.Request.IsRemovable?? false)
        {
            await _source.DeleteMessageAsync(lastSentMessage.Response, cancellationToken);
            message = await _source.SendTextMessageAsync(messageToSend, cancellationToken: cancellationToken);
            messageToSend.MessageId = message.Id;
        }
        else
        {
            message = await _source.SendTextMessageAsync(messageToSend, cancellationToken: cancellationToken);
            messageToSend.MessageId = message.Id;
        }

        if (messageToSend.IsEditable || messageToSend.IsRemovable)
        {
            _chatToLastSentMessage[messageToSend.ChatId] = new TelegramResponse()
                { Request = messageToSend, Response = message };
        }
        else
        {
            _chatToLastSentMessage.TryRemove(messageToSend.ChatId, out var _);
        }

        return message;
    }

    public Task<IMessage> EditSentTextMessageAsync(IMessageToSend editedMessage, CancellationToken cancellationToken = default)
    {
        return _source.EditSentTextMessageAsync(editedMessage, cancellationToken);
    }

    public Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default)
    {
        return _source.SetMyCommandsAsync(buttons, cancellationToken);
    }

    public Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        return _source.DeleteMessageAsync(message, cancellationToken);
    }

    public Task<IFile?> GetFileAsync(string fileId, string? mimeType, CancellationToken cancellationToken = default)
    {
        return _source.GetFileAsync(fileId, mimeType, cancellationToken);
    }

    public Task<TelegramWebHookInfo> GetWebhookInfoAsync()
    {
        return _source.GetWebhookInfoAsync();
    }

    public Task SetWebhookAsync(string url)
    {
        return _source.SetWebhookAsync(url);
    }
    
    internal class TelegramResponse
    {
        internal IMessageToSend Request;
        internal IMessage Response;
    }
}