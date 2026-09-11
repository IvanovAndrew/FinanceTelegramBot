using Application.Core;
using MediatR;

namespace Application.Bot.Statistic;

public class NeitherIncomesNotOutcomesFoundEvent : INotification
{
    public long SessionId { get; init; }
}

public class NeitherIncomesNotOutcomesFoundEventHandler(IConversation conversation) : INotificationHandler<NeitherIncomesNotOutcomesFoundEvent>
{
    public async Task Handle(NeitherIncomesNotOutcomesFoundEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify("There is no any expenses for this period"), cancellationToken);
    }
}