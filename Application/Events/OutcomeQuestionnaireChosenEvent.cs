using MediatR;

namespace Application.Events;

public class OutcomeQuestionnaireChosenEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class OutcomeQuestionnaireChosenEventHandler(
    IDateTimeService dateTimeService,
    IMessageService messageService)
    : INotificationHandler<OutcomeQuestionnaireChosenEvent>
{
    public async Task Handle(OutcomeQuestionnaireChosenEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId, 
                Id = notification.LastSentMessageId, 
                Text = "Enter the date",
                Options = MessageOptions.FromList(new []
                {
                    new Option(dateTimeService.Today().ToString("dd.MM.yyyy"), "Today"),
                    new Option(dateTimeService.Today().AddDays(-1).ToString("dd.MM.yyyy"), "Yesterday"),
                    new Option("Another day"),
                })
            },
            cancellationToken);
    }
}