using System.Collections.Concurrent;
using Application.Bot;
using Application.Core;

namespace Infrastructure;

public class TelegramConversation(IMessageService telegram, IConversationStateStore state) : IConversation
{
    public async Task Update(long sessionId, Screen screen, CancellationToken ct)
    {
        var (activeMessage, mode) = state.GetActiveMessageId(sessionId);

        bool sendMessage = true;
        int messageId = -1;
        if (activeMessage is {} activeMessageId)
        {
            messageId = activeMessageId;
            if (mode == ScreenMode.Update)
            {
                if (screen.Bytes is { Length: > 0 })
                {
                    await telegram.SendPictureAsync(sessionId, screen.Bytes, screen.Text, cancellationToken: ct);
                }
                else
                {
                    await telegram.EditSentTextMessageAsync(sessionId, activeMessageId, screen.Text, screen.Options, screen.Table, useMarkdown:screen.UseMarkdown, cancellationToken: ct);
                }
                
                sendMessage = false;
            }
            else if (mode == ScreenMode.Replace)
            {
                await telegram.DeleteMessageAsync(sessionId, activeMessageId, ct);
            }
        }
        
        if (sendMessage)
        {
            int newMessageId = 0;
            if (screen.Bytes is { Length: > 0 })
            {
                newMessageId = await telegram.SendPictureAsync(sessionId, screen.Bytes, screen.Text, cancellationToken: ct);
            }
            else
            {
                newMessageId = await telegram.SendTextMessageAsync(sessionId, screen.Text, screen.Options, screen.Table, screen.UseMarkdown, cancellationToken: ct);
            }
            
            messageId = newMessageId;
        }

        state.SetActiveMessageId(sessionId, messageId, screen.Mode);
    }

    public async Task Finish(long sessionId, CancellationToken ct)
    {
        var (activeMessage, mode) = state.GetActiveMessageId(sessionId);

        if (activeMessage is {} messageId)
        {
            switch (mode)
            {
                case ScreenMode.Update:
                case ScreenMode.Replace:
                    await telegram.RemoveMessageButtonsAsync(sessionId, messageId, ct);
                    break;
                
                default:break;
            }
        }
        
        state.Clear(sessionId);
    }
}

