using MediatR;

namespace Application.Events;

public record AskFiscalNumberEvent : INotification
{
    public long SessionId { get; init; }
}

public class AddCheckTotalPriceSavedEventHandler(IConversation conversation) : INotificationHandler<AskFiscalNumberEvent>
{
    public async Task Handle(AskFiscalNumberEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterFiscalNumber(), cancellationToken);
    }
}