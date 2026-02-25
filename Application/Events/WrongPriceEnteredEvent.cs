using MediatR;

namespace Application.Events;

public record WrongPriceEnteredEvent : INotification
{
    public long SessionId { get; init; }
    public string Error { get; init; }
}

public class WrongPriceEnteredEventHandler(IConversation conversation)
    : INotificationHandler<WrongPriceEnteredEvent>
{
    public async Task Handle(WrongPriceEnteredEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify($"{notification.Error}. Try again"), cancellationToken);
    }
}