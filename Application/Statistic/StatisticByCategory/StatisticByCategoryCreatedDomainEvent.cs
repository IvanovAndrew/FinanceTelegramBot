using Application.Events;
using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategoryCreatedEventHandler(
    IUserSessionService userSessionService,
    ICategoryProvider categoryProvider,
    IMessageService messageService)
    : INotificationHandler<StatisticByCategoryCreatedEvent>
{
    public async Task Handle(StatisticByCategoryCreatedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
                {
                    ChatId = notification.SessionId,
                    Id = session.LastSentMessageId,
                    Text = "Enter the category",
                    Options = MessageOptions.FromList(categoryProvider.GetCategories(false).Select(c => c.Name).ToArray())
                }, cancellationToken);
        }
    }
}