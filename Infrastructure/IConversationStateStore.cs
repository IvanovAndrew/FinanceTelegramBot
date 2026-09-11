using System.Collections.Concurrent;
using Application.Core;

namespace Infrastructure;

public interface IConversationStateStore
{
    IReadOnlyList<long> GetActiveChats();
    (int?, ScreenMode) GetActiveMessageId(long sessionId);
    void SetActiveMessageId(long sessionId, int messageId, ScreenMode mode);
    void Clear(long sessionId);
}

public class ConversationStateStore : IConversationStateStore
{
    private readonly ConcurrentDictionary<long, (int?, ScreenMode)> activeMessageIds =
        new ConcurrentDictionary<long, (int?, ScreenMode)>();

    public IReadOnlyList<long> GetActiveChats()
    {
        return activeMessageIds.Keys.ToList();
    }

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