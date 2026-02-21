using MediatR;

namespace Application.Events;

public record AskFiscalNumberEvent : INotification
{
    public long SessionId { get; init; }
}

public class AddCheckTotalPriceSavedEventHandler(IMessageService messageService) : INotificationHandler<AskFiscalNumberEvent>
{
    public async Task Handle(AskFiscalNumberEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the fiscal number. It should contain 16 digits"
        }, cancellationToken);
    }
}