using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByMonth;

public class StatisticByMonthSaveDateSavedEventHandler(IUserSessionService userSessionService, IMessageService messageService, ILogger<StatisticByMonthSaveDateSavedEventHandler> logger) : INotificationHandler<StatisticByMonthSaveDateSavedEvent>
{
    public async Task Handle(StatisticByMonthSaveDateSavedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(StatisticByMonthSaveDateSavedEventHandler)} is called");
        
        var userSession = userSessionService.GetUserSession(notification.SessionId);

        if (userSession != null)
        {
            var message = await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = notification.SessionId,
                    Id = userSession.LastSentMessageId,
                    Text = "Enter the currency",
                    Options = MessageOptions.FromListAndLastSingleLine(Currency.GetAvailableCurrencies().Select(c => c.Name).ToList(), "All")
                },
            cancellationToken);

            userSession.LastSentMessageId = message.Id;
        }
        
        logger.LogInformation($"{nameof(StatisticByMonthSaveDateSavedEventHandler)} finished");
    }
}