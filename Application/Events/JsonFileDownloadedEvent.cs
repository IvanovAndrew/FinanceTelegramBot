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
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(notification.Json));
        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);
        var cleanJson = await reader.ReadToEndAsync(cancellationToken);
        
        var expenses = parser.ParseOutcomes(cleanJson, Categories.Outcome.DefaultCategory);

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

public class NoExpensesInJsonEventHandler(IMessageService messageService) : INotificationHandler<NoExpensesInJsonEvent>
{
    public async Task Handle(NoExpensesInJsonEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message(){ ChatId = notification.SessionId, Text = "There are no expenses in json file" }, cancellationToken);
    }
}