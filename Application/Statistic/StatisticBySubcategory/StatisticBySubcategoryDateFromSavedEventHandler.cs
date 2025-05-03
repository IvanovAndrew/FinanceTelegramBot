using Domain;
using Domain.Events;
using MediatR;

namespace Application.Statistic;

public class StatisticBySubcategoryDateFromSavedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<StatisticBySubcategoryDateFromSavedEvent>
{
    public async Task Handle(StatisticBySubcategoryDateFromSavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the currency",
                    Options = MessageOptions.FromListAndLastSingleLine(Currency.GetAvailableCurrencies().Select(c => c.Name).ToList(), "All")
                }
                , cancellationToken);
        }
    }
}