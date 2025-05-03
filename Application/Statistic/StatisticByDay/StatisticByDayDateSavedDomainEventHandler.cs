using Domain;
using Domain.Events;
using MediatR;

namespace Application.Events;

public class StatisticByDayDateSavedDomainEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<StatisticByDayDateSavedEvent>
{
    public async Task Handle(StatisticByDayDateSavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = notification.SessionId,
                    Id = session.LastSentMessageId,
                    Text = "Enter the currency",
                    Options = MessageOptions.FromListAndLastSingleLine(Currency.GetAvailableCurrencies().Select(c => c.Name).ToList(), "All")
                }, cancellationToken);
        }
    }
}