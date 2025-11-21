using Application.Statistic.StatisticByDay;
using Infrastructure;
using MediatR;

namespace Application.Events;

public class StatisticByDayCreatedDomainEventHandler(IUserSessionService userSessionService, IMessageService messageService, IDateTimeService dateTimeService)
    : INotificationHandler<StatisticByDayCreatedEvent>
{
    public async Task Handle(StatisticByDayCreatedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = notification.SessionId,
                    Id = session.LastSentMessageId,
                    Text = "Enter the day",
                    Options = MessageOptions.FromList(new List<string>(){"Today", "Yesterday", "Another day"})
                }
                , cancellationToken);
        }
    }
}