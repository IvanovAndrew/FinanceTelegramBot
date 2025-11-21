using Application.Statistic.StatisticBySubcategory;
using MediatR;

namespace Application.Statistic;

public class StatisticBySubcategoryCreatedEventHandler(IUserSessionService userSessionService, ICategoryProvider categoryProvider, IMessageService messageService) : INotificationHandler<StatisticBySubcategoryCreatedEvent>
{
    public async Task Handle(StatisticBySubcategoryCreatedEvent notification, CancellationToken cancellationToken)
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
                    Options = MessageOptions.FromList(categoryProvider.GetCategories(false).Select(c => c.Name).ToArray()),
                }, cancellationToken);
        }
    }
}