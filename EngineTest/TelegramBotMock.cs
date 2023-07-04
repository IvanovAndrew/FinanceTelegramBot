﻿using Infrastructure;

namespace EngineTest;

public class TelegramBotMock : ITelegramBot
{
    private int _messageId = 0;
    private readonly List<IMessage> _messages = new();
    public IReadOnlyList<IMessage> SentMessages => _messages.AsReadOnly();

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

    public async Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default)
    {
        await Task.Delay(5, cancellationToken);
    }

    public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken)
    {
        var messageToRemove = _messages.FirstOrDefault(m => m.Id == messageId);
        _messages.Remove(messageToRemove);
        return Task.Run(() => {});
    }
}