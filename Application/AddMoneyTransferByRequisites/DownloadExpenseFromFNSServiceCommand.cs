using Application.Contracts;
using Application.Services;
using Domain;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record DownloadExpenseFromFNSServiceCommand : IRequest
{
    public long SessionId { get; init; }
    public CheckRequisite CheckRequisite { get; init; }
}

public class DownloadExpenseFromFNSServiceCommandHandler(IUserSessionService userSessionService, ICheckDownloader checkDownloader, IExpenseCategorizer expenseCategorizer, IExpenseCategoryMappingCache expenseCategoryMappingCache, IProgressNotifier progressNotifier, IMediator mediator) : IRequestHandler<DownloadExpenseFromFNSServiceCommand>
{
    public async Task Handle(DownloadExpenseFromFNSServiceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await progressNotifier.Start(request.SessionId, "Loading category mapping...", cancellationToken);
            
            var mapping = await expenseCategoryMappingCache.Get(Currency.RUR, cancellationToken);
            
            await progressNotifier.Update(request.SessionId, "Downloading the outcomes from FNS service", cancellationToken);
        
            var outcomes = await checkDownloader.DownloadExpenses(request.CheckRequisite, expenseCategorizer, mapping, Categories.Outcome.DefaultCategory);
            
            await progressNotifier.Update(request.SessionId, "Expenses are successfully downloaded from FNS service", cancellationToken);
            
            await mediator.Send(new SaveOutcomesBatchCommand() { SessionId = session.Id, MoneyTransfers = outcomes }, cancellationToken);
        }
    }
}