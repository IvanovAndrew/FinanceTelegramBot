using MediatR;

namespace Application.Events;

public class WrongFileExtensionReceivedEventHandler(IConversation conversation) : INotificationHandler<WrongFileExtensionReceivedEvent>
{
    public async Task Handle(WrongFileExtensionReceivedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify("Paste a json file"), cancellationToken);
    }
}