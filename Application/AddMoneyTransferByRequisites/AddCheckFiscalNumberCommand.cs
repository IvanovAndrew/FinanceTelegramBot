using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record CheckFiscalNumberNotSavedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalNumberNotSavedEventHandler(IMessageService messageService) : INotificationHandler<CheckFiscalNumberNotSavedEvent>
{
    public async Task Handle(CheckFiscalNumberNotSavedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the fiscal number. It should contain 16 digits"
        }, cancellationToken);
    }
}

public class AskFiscalDocumentNumberEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalNumberSavedEventHandler(IMessageService messageService) : INotificationHandler<AskFiscalDocumentNumberEvent>
{
    public async Task Handle(AskFiscalDocumentNumberEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the document number"
        }, cancellationToken);
    }
}