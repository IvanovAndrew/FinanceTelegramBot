using MediatR;

namespace Application.Events;

public class WrongFileExtensionReceivedEventHandler(IMessageService messageService) : INotificationHandler<WrongFileExtensionReceivedEvent>
{
    public async Task Handle(WrongFileExtensionReceivedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Text = "Paste a json file"
            }, cancellationToken);
    }
}