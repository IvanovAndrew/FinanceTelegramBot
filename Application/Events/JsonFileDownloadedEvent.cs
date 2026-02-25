using Application.AddMoneyTransferByRequisites;
using Domain;
using MediatR;

namespace Application.Events;

public class JsonFileDownloadedEvent : INotification
{
    public long SessionId { get; init; }
    public string Json { get; init; }
}

public class JsonFileDownloadedEventHandler(IExpenseJsonParser parser, IMediator mediator) : INotificationHandler<JsonFileDownloadedEvent>
{
    public async Task Handle(JsonFileDownloadedEvent notification, CancellationToken cancellationToken)
    {
        var expenses = parser.ParseOutcomes(notification.Json, Categories.Outcome.DefaultCategory);

        if (!expenses.Any())
        {
            await mediator.Publish(new NoExpensesInJsonEvent()
                { SessionId = notification.SessionId }, cancellationToken);
            return;
        }
        
        await mediator.Send(new SaveOutcomesBatchCommand()
            { SessionId = notification.SessionId, MoneyTransfers = expenses }, cancellationToken);
    }
}

public record NoExpensesInJsonEvent : INotification
{
    public long SessionId { get; init; }
}

public class NoExpensesInJsonEventHandler(IConversation conversation) : INotificationHandler<NoExpensesInJsonEvent>
{
    public async Task Handle(NoExpensesInJsonEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify("There are no expenses in json file"), cancellationToken);
    }
}