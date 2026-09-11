using Application.Core;
using MediatR;

namespace Application.Bot;

public record YerevanCityCheckRequisite
{
    public DateOnly Date { get; init; }
    public string OrderId { get; init; }
}

public record DownloadExpensesFromYerevanCityServiceCommand : IRequest
{
    public long SessionId { get; init; }
    public YerevanCityCheckRequisite CheckRequisite { get; init; }
}

public class DownloadExpensesFromYerevanCityServiceCommandHandler(IUserSessionService userSessionService, IYerevanCityReceiptProvider yerevanCityReceiptProvider, IConversation conversationUi, IMediator mediator) : IRequestHandler<DownloadExpensesFromYerevanCityServiceCommand>
{
    public async Task Handle(DownloadExpensesFromYerevanCityServiceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await conversationUi.Update(request.SessionId, Screens.NotifyOperationInProgress("Downloading the outcomes from Yerevan city service"), cancellationToken);
        
            var check = await yerevanCityReceiptProvider.ProcessAsync(request.CheckRequisite.Date, request.CheckRequisite.OrderId, cancellationToken);
            
            await conversationUi.Update(request.SessionId, Screens.NotifyOperationInProgress("Expenses are successfully downloaded from Yerevan city service"), cancellationToken);
            
            await mediator.Send(new SaveOutcomesBatchCommand() { SessionId = session.Id, MoneyTransfers = check.Outcomes }, cancellationToken);
        }
    }
}