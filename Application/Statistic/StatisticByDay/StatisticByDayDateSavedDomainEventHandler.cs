using MediatR;

namespace Application.Statistic.StatisticByDay;

public class StatisticByDayDateSavedDomainEventHandler(ICurrencyProvider currencyProvider, IConversation conversation) : INotificationHandler<StatisticByDayDateSavedEvent>
{
    public async Task Handle(StatisticByDayDateSavedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(
            notification.SessionId, Screens.SelectCurrency(currencyProvider.GetCurrencies()), cancellationToken);
    }
}