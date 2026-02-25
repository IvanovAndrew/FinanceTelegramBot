using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class AskFiscalDocumentSignEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalDocumentNumberSavedHandler(IConversation conversation) : INotificationHandler<AskFiscalDocumentSignEvent>
{
    public async Task Handle(AskFiscalDocumentSignEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.EnterFiscalDocumentSign(), cancellationToken);
    }
}