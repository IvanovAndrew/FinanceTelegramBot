using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class AskFiscalDocumentSignEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalDocumentNumberSavedHandler(IMessageService messageService) : INotificationHandler<AskFiscalDocumentSignEvent>
{
    public async Task Handle(AskFiscalDocumentSignEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the fiscal document sign. It should contain only digits",
        }, cancellationToken);
    }
}