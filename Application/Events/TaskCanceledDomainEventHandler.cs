using MediatR;

namespace Application.Events;

public class TaskCanceledDomainEventHandler(IMessageService messageService) : INotificationHandler<TaskCanceledEvent>
{
    public async Task Handle(TaskCanceledEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Text = "The operation is cancelled"
            }, cancellationToken);
    }
}