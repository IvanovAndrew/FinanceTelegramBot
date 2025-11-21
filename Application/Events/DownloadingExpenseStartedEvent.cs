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
            var sentMessage = await messageService.SendTextMessageAsync(new Message()
            {
                ChatId = session.Id,
                Text = "Downloading the outcomes from FNS service",
            }, cancellationToken);

            session.LastSentMessageId = sentMessage.Id;
        }
    }
}

public class DownloadingExpenseFinishedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<DownloadingExpenseStartedEvent>
{
    public async Task Handle(DownloadingExpenseStartedEvent notification, CancellationToken cancellationToken)
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