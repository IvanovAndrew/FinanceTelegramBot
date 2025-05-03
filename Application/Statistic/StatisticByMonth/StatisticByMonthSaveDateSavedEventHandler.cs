using Domain;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Events;

public class StatisticByMonthSaveDateSavedEventHandler(IUserSessionService userSessionService, IMessageService messageService, ILogger<StatisticByMonthSaveDateSavedEventHandler> logger) : INotificationHandler<StatisticByMonthSaveDateSavedEvent>
{
    public async Task Handle(StatisticByMonthSaveDateSavedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("StatisticByMonthSaveDateSavedEventHandler is called");
        var userSession = userSessionService.GetUserSession(notification.SessionId);

        if (userSession != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = notification.SessionId,
                    Id = userSession.LastSentMessageId,
                    Text = "Enter the currency",
                    Options = MessageOptions.FromListAndLastSingleLine(Currency.GetAvailableCurrencies().Select(c => c.Name).ToList(), "All")
                },
            cancellationToken);
        }
    }
}