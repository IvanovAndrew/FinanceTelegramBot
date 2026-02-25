using MediatR;

namespace Application.Statistic.StatisticByDay;

public class StatisticByDayCreatedDomainEventHandler(IConversation messageService, IDateTimeService dateTimeService)
    : INotificationHandler<StatisticByDayCreatedEvent>
{
    public async Task Handle(StatisticByDayCreatedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.Update(notification.SessionId,
            Screens.SelectDay(dateTimeService.Today()), cancellationToken);
    }
}