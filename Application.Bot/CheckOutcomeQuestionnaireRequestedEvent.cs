using Application.Core;
using MediatR;

namespace Application.Bot;

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