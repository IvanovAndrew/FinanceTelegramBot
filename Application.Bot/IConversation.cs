using Application.Core;

namespace Application.Bot;

public interface IConversation
{
    Task Update(long sessionId, Screen screen, CancellationToken ct);
    Task Finish(long sessionId, CancellationToken ct);
}