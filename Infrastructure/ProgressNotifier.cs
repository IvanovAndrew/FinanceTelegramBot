using System.Collections.Concurrent;
using Application;

namespace Infrastructure;

public sealed class TelegramProgressNotifier(IMessageService messageService) : IProgressNotifier
{
    private readonly ConcurrentDictionary<long, int> _progressMessages = new();

    public async Task Start(long sessionId, string text, CancellationToken ct)
    {
        var messageId = await messageService.SendTextMessageAsync(sessionId, text, cancellationToken: ct);

        _progressMessages[sessionId] = messageId;
    }

    public async Task Update(long sessionId, string text, CancellationToken ct)
    {
        if (!_progressMessages.TryGetValue(sessionId, out var messageId))
            return;

        await messageService.EditSentTextMessageAsync(
            sessionId,
            messageId,
            text,
            cancellationToken:ct);
    }

    public async Task Finish(long sessionId, string text, CancellationToken ct)
    {
        if (!_progressMessages.TryRemove(sessionId, out var messageId))
            return;

        await messageService.EditSentTextMessageAsync(
            sessionId,
            messageId,
            text,
            cancellationToken: ct);
    }
}