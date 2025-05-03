using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryByMonthCreatedEventHandler(IUserSessionService userSessionService, ICategoryProvider categoryProvider, IMessageService messageService) : INotificationHandler<StatisticBySubcategoryByMonthCreatedEvent>
{
    public async Task Handle(StatisticBySubcategoryByMonthCreatedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the category",
                    Options = MessageOptions.FromList(categoryProvider.GetCategories(false).Where(c => c.Subcategories.Any())
                        .Select(c => c.Name).ToList())
                }, cancellationToken);
        }
    }
}