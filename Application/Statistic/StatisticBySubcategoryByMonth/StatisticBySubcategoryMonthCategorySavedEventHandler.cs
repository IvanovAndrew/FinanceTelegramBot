using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthCategorySavedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<AskStatisticSubCategoryEvent>
{
    public async Task Handle(AskStatisticSubCategoryEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the subcategory",
                Options = MessageOptions.FromList(notification.SubCategories.Select(sc => sc.Name).ToList()),
            },
            cancellationToken
        );
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
                            today.ToString(DateFormat.FullMonthName), 
                            today.AddMonths(-6).ToString(DateFormat.FullMonthName)
                        }, "Another month")
                },
                cancellationToken
            );
        }
    }
}