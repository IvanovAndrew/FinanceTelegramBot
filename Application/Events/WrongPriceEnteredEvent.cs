using MediatR;

namespace Application.Events;

public record WrongPriceEnteredEvent : INotification
{
    public long SessionId { get; init; }
    public string Error { get; init; }
}

public class WrongPriceEnteredEventHandler(IMessageService messageService)
    : INotificationHandler<WrongPriceEnteredEvent>
{
    public async Task Handle(WrongPriceEnteredEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Text = $"{notification.Error}. Try again",
            }, cancellationToken);
    }
}