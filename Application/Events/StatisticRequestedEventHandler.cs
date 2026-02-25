using MediatR;

namespace Application.Events;

public class StatisticRequestedEventHandler(IConversation conversation)
    : INotificationHandler<StatisticRequestedEvent>
{
    public async Task Handle(StatisticRequestedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.SelectStatistic(), cancellationToken);
    }
}