using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthCategorySavedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<StatisticBySubcategoryMonthCategorySavedEvent>
{
    public async Task Handle(StatisticBySubcategoryMonthCategorySavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the subcategory",
                    Options = MessageOptions.FromList(session.StatisticsOptions.Category.Subcategories.Select(sc => sc.Name).ToList()),
                },
                cancellationToken
            );
        }
    }
}

public class StatisticBySubcategoryMonthSubcategorySavedDomainEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMessageService messageService) : INotificationHandler<StatisticBySubcategoryMonthSubcategorySavedEvent>
{
    public async Task Handle(StatisticBySubcategoryMonthSubcategorySavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var today = dateTimeService.Today();
            
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the month",
                    Options = MessageOptions.FromListAndLastSingleLine(
                        new [] 
                        { 
                            today.ToString("MMMM yyyy"), 
                            today.AddMonths(-6).ToString("MMMM yyyy")
                        }, "Another month")
                },
                cancellationToken
            );
        }
    }
}