using Application.Bot;
using Application.Core;

namespace Application.Test.Stubs;

public class ConversationStub(IMessageService messageService) : IConversation
{
    private int? _lastSentMessageId = null;
    
    public async Task Update(long sessionId, Screen screen, CancellationToken ct)
    {
        if (screen.Mode == ScreenMode.Update && _lastSentMessageId is not null)
        {
            _lastSentMessageId = await messageService.EditSentTextMessageAsync(sessionId, _lastSentMessageId.Value, screen.Text, options:screen.Options, table:screen.Table, cancellationToken: ct);
        }
        else
        {
            _lastSentMessageId = await messageService.SendTextMessageAsync(sessionId, screen.Text, options:screen.Options, table:screen.Table, cancellationToken: ct);
        }
    }

    public Task Finish(long sessionId, CancellationToken ct)
    {
        _lastSentMessageId = null;
        return Task.CompletedTask;
    }
}