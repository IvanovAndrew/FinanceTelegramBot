using Application.Bot;
using Application.Test.Stubs;

namespace Application.Test.Extensions;

internal class BotEngineWrapper(BotEngine botEngine, MessageServiceMock messageService)
{
    internal async Task<IOutcomingMessage> Proceed(string text)
    {
        var lastSendMessage = messageService.SentMessages.LastOrDefault();

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
        
        await botEngine.Proceed(new IncomingMessageStub() { Text = messageText }, default);
        
        var lastMessage = messageService.SentMessages.OrderBy(m => m.Id).Last();
            
        return lastMessage;
    }
    
    internal async Task<IOutcomingMessage> ProceedFile(FileInfoStub fileInfo)
    {
        var message = new IncomingMessageStub
        {
            FileInfo = fileInfo
        };

        await botEngine.Proceed(message, default);

        var lastMessage = messageService.SentMessages.OrderBy(m => m.Id).Last();
            
        return lastMessage;
    }
}