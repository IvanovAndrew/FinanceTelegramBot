using Application.Services;
using Application.Test.Stubs;
using UnitTest;

namespace Application.Test.Extensions;

internal class BotEngineWrapper
{
    private readonly BotEngine _botEngine;
    private readonly MessageServiceMock _messageService;
    
    public BotEngineWrapper(BotEngine botEngine, MessageServiceMock messageService)
    {
        _botEngine = botEngine;
        _messageService = messageService;
    }

    internal async Task<IMessage> Proceed(string text)
    {
        var lastSendMessage = _messageService.SentMessages.LastOrDefault();

        var messageText = text;

        if (!messageText.StartsWith("/") && (lastSendMessage?.Options?.Any() ?? false))
        {
            messageText = lastSendMessage.Options
                .FirstOrDefault(b => string.Equals(b.Text, text, StringComparison.InvariantCultureIgnoreCase))?.Code;

            if (messageText == null)
            {
                throw new InvalidOperationException( 
                    $"Couldn't find {text} option between {(string.Join(", ", lastSendMessage.Options.Select(o => o.Text)))}");
            }
        }
        
        await _botEngine.Proceed(new MessageStub() { Text = messageText }, default);
        
        var lastMessage = _messageService.SentMessages.OrderBy(m => m.Id).Last();
            
        return lastMessage;
    }
    
    internal async Task<IMessage> ProceedFile(FileInfoStub fileInfo)
    {
        var message = new MessageStub
        {
            FileInfo = fileInfo
        };

        await _botEngine.Proceed(message, default);

        var lastMessage = _messageService.SentMessages.OrderBy(m => m.Id).Last();
            
        return lastMessage;
    }
}