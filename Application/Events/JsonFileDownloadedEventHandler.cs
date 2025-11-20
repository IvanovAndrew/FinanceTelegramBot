using Application.AddMoneyTransferByRequisites;
using Domain;
using MediatR;

namespace Application.Events;

public class JsonFileDownloadedEventHandler(IExpenseJsonParser parser, ICategoryProvider categoryProvider, IMediator mediator) : INotificationHandler<JsonFileDownloadedEvent>
{
    public async Task Handle(JsonFileDownloadedEvent notification, CancellationToken cancellationToken)
    {
        var expenses = parser.Parse(notification.Json, categoryProvider.DefaultOutcomeCategory(), Currency.RUR);
        
        await mediator.Send(new SaveOutcomesBatchCommand()
            { SessionId = notification.SessionId, MoneyTransfers = expenses }, cancellationToken);
    }
}