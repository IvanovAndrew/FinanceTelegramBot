using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record CheckFiscalNumberNotSavedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalNumberNotSavedEventHandler(IConversation conversation) : INotificationHandler<CheckFiscalNumberNotSavedEvent>
{
    public async Task Handle(CheckFiscalNumberNotSavedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterFiscalNumber(), cancellationToken);
    }
}

public class AskFiscalDocumentNumberEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalNumberSavedEventHandler(IConversation conversation) : INotificationHandler<AskFiscalDocumentNumberEvent>
{
    public async Task Handle(AskFiscalDocumentNumberEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterFiscalDocumentNumber(), cancellationToken);
    }
}