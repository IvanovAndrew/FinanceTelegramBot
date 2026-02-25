namespace Application;

public interface IProgressNotifier
{
    Task Start(long sessionId, string text, CancellationToken ct);
    Task Update(long sessionId, string text, CancellationToken ct);
    Task Finish(long sessionId, string text, CancellationToken ct);
}

public interface IConversation
{
    Task Update(long sessionId, Screen screen, CancellationToken ct);
}