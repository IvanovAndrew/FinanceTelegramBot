using MediatR;

namespace Application.Events;

public class StatisticRequestedEventHandler(IMessageService messageService, IUserSessionService userSessionService)
    : INotificationHandler<StatisticRequestedEvent>
{
    public async Task Handle(StatisticRequestedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = notification.SessionId,
                Id = session.LastSentMessageId,
                Text = "Choose kind of statistic",
                Options = MessageOptions.FromList(new []
                {
                    new Option ("/balance", "Balance"),
                    new Option ("/statisticByDay", "Day expenses (by categories)"),
                    new Option ("/statisticByMonth", "Month expenses (by categories)"), 
                    new Option ("/statisticByCategory", "Category expenses (by months)"), 
                    new Option ("/statisticBySubcategory", "Subcategory expenses (overall)"), 
                    new Option ("/statisticBySubcategoryByMonth", "Subcategory expenses (by months)"), 
                })
            }, cancellationToken);
        }
    }
}