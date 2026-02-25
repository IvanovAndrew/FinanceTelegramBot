using System.Collections.Concurrent;
using Application;

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
                await telegram.EditSentTextMessageAsync(sessionId, activeMessageId, screen.Text, screen.Options, screen.Table, cancellationToken: ct);
                sendMessage = false;
            }
            else if (mode == ScreenMode.Replace)
            {
                await telegram.DeleteMessageAsync(sessionId, activeMessageId, ct);
            }
        }
        
        if (sendMessage)
        {
            var newMessageId = await telegram.SendTextMessageAsync(sessionId, screen.Text, screen.Options, screen.Table, cancellationToken: ct);
            messageId = newMessageId;
        }

        state.SetActiveMessageId(sessionId, messageId, screen.Mode);
    }
}

public interface IConversationStateStore
{
    (int?, ScreenMode) GetActiveMessageId(long sessionId);
    void SetActiveMessageId(long sessionId, int messageId, ScreenMode mode);
    void Clear(long sessionId);
}

public class ConversationStateStore : IConversationStateStore
{
    private readonly ConcurrentDictionary<long, (int?, ScreenMode)> activeMessageIds =
        new ConcurrentDictionary<long, (int?, ScreenMode)>();

    public (int?, ScreenMode) GetActiveMessageId(long sessionId)
    {
        return activeMessageIds.GetValueOrDefault(sessionId);
    }

    public void SetActiveMessageId(long sessionId, int messageId, ScreenMode mode)
    {
        activeMessageIds[sessionId] = (messageId, mode);
    }

    public void Clear(long sessionId)
    {
        activeMessageIds.Remove(sessionId, out _);
    }
}