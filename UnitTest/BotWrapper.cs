using StateMachine;

namespace UnitTest;

internal class BotEngineWrapper
{
    private readonly BotEngine _botEngine;
    private readonly TelegramBotMock _telegramBot;
    
    internal BotEngineWrapper(BotEngine botEngine, TelegramBotMock telegramBot)
    {
        _botEngine = botEngine;
        _telegramBot = telegramBot;
    }

    internal async Task<MessageStub> Proceed(string text)
    {
        var lastSendMessage = _telegramBot.SentMessages.LastOrDefault() as MessageStub;

        var messageText = text;
        if (lastSendMessage?.TelegramKeyboard != null)
        {
            messageText = lastSendMessage.TelegramKeyboard.Buttons.SelectMany(row => row.Select(b => b))
                .First(b => string.Equals(b.Text, text, StringComparison.InvariantCultureIgnoreCase)).CallbackData;
        }
        
        var lastMessage = await _botEngine.Proceed(new MessageStub() { Text = messageText }, _telegramBot) as MessageStub;
            
        return lastMessage!;
    }
    
    internal async Task<MessageStub> ProceedFile(FileInfoStub fileInfo)
    {
        var message = new MessageStub
        {
            FileInfo = fileInfo
        };

        var lastMessage = await _botEngine.Proceed(message, _telegramBot) as MessageStub;
            
        return lastMessage!;
    }
}