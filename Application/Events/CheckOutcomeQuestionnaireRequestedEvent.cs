using MediatR;

namespace Application.Events;

public record CheckOutcomeQuestionnaireRequestedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckOutcomeQuestionnaireRequestedEventHandler(
    IConversation conversation) : INotificationHandler<
    CheckOutcomeQuestionnaireRequestedEvent>
{
    public async Task Handle(CheckOutcomeQuestionnaireRequestedEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(
            notification.SessionId,
            Screens.SelectCheckSource(),
            cancellationToken);
    }
}