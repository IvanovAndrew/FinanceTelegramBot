using Application.Commands.SaveOutcomesBatch;
using Domain;
using MediatR;

namespace Application.Events;

public class JsonFileDownloadedEventHandler(IExpenseJsonParser parser, IMediator mediator) : INotificationHandler<JsonFileDownloadedEvent>
{
    public async Task Handle(JsonFileDownloadedEvent notification, CancellationToken cancellationToken)
    {
        var expenses = parser.Parse(notification.Json, "Еда", Currency.Rur);
        
        await mediator.Send(new SaveOutcomesBatchCommand()
            { SessionId = notification.SessionId, MoneyTransfers = expenses }, cancellationToken);
    }
}