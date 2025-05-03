using Application.AddMoneyTransfer;
using MediatR;

namespace Application.Events;

public class MoneyTransferSavingStartedDomainEventHandler(IMessageService messageService) : INotificationHandler<MoneyTransferSavingStartedEvent>
{
    public async Task Handle(MoneyTransferSavingStartedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.MessageId,
                Text = "Saving..."
            }, cancellationToken);
    }
}