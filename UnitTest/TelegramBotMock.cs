using Infrastructure.Telegram;

namespace UnitTest;

public class TelegramBotMock : ITelegramBot
{
    private int _messageId = 0;
    private readonly List<IMessage> _messages = new();
    public IReadOnlyList<IMessage> SentMessages => _messages.AsReadOnly();
    public Dictionary<string, FileStub> SavedFiles = new();

    public Task<IMessage> SendTextMessageAsync(long chatId, string text)
    {
        var message = new MessageStub(){Id = _messageId++, ChatId = chatId, Text = text, Date = DateTime.Now};
        _messages.Add(message);
        return Task.FromResult<IMessage>(message);
    }

    public Task<IMessage> SendTextMessageAsync(long chatId, string text, TelegramKeyboard? keyboard = null, bool useMarkdown = false,
        CancellationToken cancellationToken = default)
    {
        var message = new MessageStub()
        {
            Id = _messageId++,
            ChatId = chatId,
            TelegramKeyboard = keyboard,
            Text = text,
            Date = DateTime.Now
        };
        _messages.Add(message);
        return Task.FromResult<IMessage>(message);
    }

    public Task<IMessage> SendTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default)
    {
        var message = new MessageStub()
        {
            Id = _messageId++,
            ChatId = messageToSend.ChatId,
            TelegramKeyboard = messageToSend.Keyboard,
            Text = messageToSend.Text,
            Date = DateTime.Now
        };
        _messages.Add(message);
        return Task.FromResult<IMessage>(message);
    }

    public Task<IMessage> EditSentTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default)
    {
        await Task.Delay(5, cancellationToken);
    }

    public Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        _messages.Remove(message);
        return Task.CompletedTask;
    }

    public Task<IFile?> GetFileAsync(string fileId, string? mimeType, CancellationToken cancellationToken)
    {
        return Task.FromResult(SavedFiles[fileId] as IFile)!;
    }

    public Task<TelegramWebHookInfo> GetWebhookInfoAsync()
    {
        throw new NotImplementedException();
    }

    public Task SetWebhookAsync(string url)
    {
        throw new NotImplementedException();
    }
}