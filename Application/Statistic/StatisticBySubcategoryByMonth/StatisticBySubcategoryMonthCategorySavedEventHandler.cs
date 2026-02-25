using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthSubcategorySavedDomainEventHandler(IDateTimeService dateTimeService, IConversation conversation) : INotificationHandler<StatisticBySubcategoryMonthSubcategorySavedEvent>
{
    public async Task Handle(StatisticBySubcategoryMonthSubcategorySavedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId,
            Screens.SelectMonth(dateTimeService.CurrentMonth()), cancellationToken);
    }
}