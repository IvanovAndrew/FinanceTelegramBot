using Application.Core;
using Application.Core.Events;
using MediatR;

namespace Application.Bot;

public class TaskCanceledDomainEventHandler(IConversation conversation) : INotificationHandler<TaskCanceledEvent>
{
    public async Task Handle(TaskCanceledEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify("The operation is cancelled"), cancellationToken);
    }
}