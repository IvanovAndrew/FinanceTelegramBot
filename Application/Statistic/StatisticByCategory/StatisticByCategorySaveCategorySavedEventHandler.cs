using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategorySaveCategorySavedEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMessageService messageService) : INotificationHandler<StatisticByCategorySaveCategorySavedEvent>
{
    public async Task Handle(StatisticByCategorySaveCategorySavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var options = new[]
                {
                    dateTimeService.Today(), dateTimeService.Today().AddMonths(-1),
                    dateTimeService.Today().AddMonths(-6)
                }
                .Select(d => d.ToString("MMMM yyyy")).ToList();
            
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the month to start with",
                    Options = MessageOptions.FromListAndLastSingleLine(options, "Another month")
                },
                cancellationToken: cancellationToken);
        }
    }
}