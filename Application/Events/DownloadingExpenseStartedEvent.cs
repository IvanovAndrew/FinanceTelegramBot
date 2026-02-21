using MediatR;

namespace Application.Events;

public record DownloadingExpenseStartedEvent : INotification
{
    public long SessionId { get; init; }
}

public record DownloadingExpenseFinishedEvent : INotification
{
    public long SessionId { get; init; }
}

public class DownloadingExpenseStartedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<DownloadingExpenseStartedEvent>
{
    public async Task Handle(DownloadingExpenseStartedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var sentMessage = await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = session.Id,
                Text = "Downloading the outcomes from FNS service",
                Id = session.LastSentMessageId,
            }, cancellationToken);

            session.LastSentMessageId = sentMessage.Id;
        }
    }
}

public class DownloadingExpenseFinishedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<DownloadingExpenseFinishedEvent>
{
    public async Task Handle(DownloadingExpenseFinishedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = session.Id,
                Id = session.LastSentMessageId,
                Text = "Expenses are successfully downloaded from FNS service",
            }, cancellationToken);
        }
    }
}

public record PreparingCategoryMappingStartedEvent : INotification
{
    public long SessionId { get; init; }
}

public class PreparingCategoryMappingStartedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<PreparingCategoryMappingStartedEvent>
{
    public async Task Handle(PreparingCategoryMappingStartedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var sentMessage = await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = session.Id,
                Text = "Loading category mapping...",
            }, cancellationToken);

            session.LastSentMessageId = sentMessage.Id;
        }
    }
}

public record CategoryMappingPreparedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CategoryMappingPreparedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<CategoryMappingPreparedEvent>
{
    public async Task Handle(CategoryMappingPreparedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var sentMessage = await messageService.EditSentTextMessageAsync(new Message()
            {
                ChatId = session.Id,
                Text = "Category mapping has been prepared",
                Id = session.LastSentMessageId,
            }, cancellationToken);

            session.LastSentMessageId = sentMessage.Id;
        }
    }
}