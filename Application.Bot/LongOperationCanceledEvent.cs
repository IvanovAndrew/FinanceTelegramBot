using Application.Core;
using MediatR;

namespace Application.Bot;

public class LongOperationCanceledEvent : INotification
{
    public long SessionId { get; init; }
}

public class LongOperationCanceledEventHandler(IConversation conversation) : INotificationHandler<LongOperationCanceledEvent>
{
    public async Task Handle(LongOperationCanceledEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify("Operation has been canceled"),
            cancellationToken);
    }
}