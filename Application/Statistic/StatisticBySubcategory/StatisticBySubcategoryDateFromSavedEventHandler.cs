using Domain;
using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticBySubcategoryDateFromSavedEventHandler(IUserSessionService userSessionService, ICurrencyProvider currencyProvider, IMessageService messageService) : INotificationHandler<StatisticBySubcategoryDateFromSavedEvent>
{
    public async Task Handle(StatisticBySubcategoryDateFromSavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var message = await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the currency",
                    Options = MessageOptions.FromListAndLastSingleLine(currencyProvider.GetCurrencies().Select(c => c.Name).ToList(), "All")
                }
                , cancellationToken);

            session.LastSentMessageId = message.Id;
        }
    }
}