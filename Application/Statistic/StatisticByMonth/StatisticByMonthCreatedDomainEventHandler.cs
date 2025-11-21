using Application.Statistic.StatisticByMonth;
using MediatR;

namespace Application.Events;

public class StatisticByMonthCreatedDomainEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMessageService messageService) : INotificationHandler<StatisticByMonthCreatedEvent>
{
    public async Task Handle(StatisticByMonthCreatedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var options = new[]
                {
                    dateTimeService.Today(), dateTimeService.Today().AddMonths(-1),
                    dateTimeService.Today().AddMonths(-6)
                }
                .Select(_ => _.ToString("MMMM yyyy")).ToList();
            options.Add("Another month");
            
            await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = session.Id,
                Id = session.LastSentMessageId,
                Text = "Enter the month",
                Options = MessageOptions.FromList(options)
            }, cancellationToken);
        }
    }
}