using Infrastructure;

namespace EngineTest;

public class TelegramBotMock : ITelegramBot
{
    private int _messageId = 0;
    
    public Task<IMessage> SendTextMessageAsync(long chatId, string text)
    {
        return Task.FromResult<IMessage>(new MessageStub(){Id = _messageId++, ChatId = chatId, Text = text, Date = DateTime.Now});
    }

    public Task<IMessage> SendTextMessageAsync(long chatId, string text, TelegramKeyboard? keyboard = null, bool useMarkdown = false,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IMessage>(new MessageStub()
        {
            Id = _messageId++,
            ChatId = chatId,
            TelegramKeyboard = keyboard,
            Text = text,
            Date = DateTime.Now
        });
    }

    public async Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default)
    {
        await Task.Delay(5, cancellationToken);
    }

    public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken)
    {
        return Task.Run(() => {});
    }
}

public class MessageStub : IMessage
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; }
    public bool Edited { get; }
    public TelegramKeyboard? TelegramKeyboard { get; set; }
}