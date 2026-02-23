using Application.Contracts;
using Application.Events;
using Application.Services;
using Domain;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record DownloadExpenseFromFNSServiceCommand : IRequest
{
    public long SessionId { get; init; }
    public CheckRequisite CheckRequisite { get; init; }
}

public class DownloadExpenseFromFNSServiceCommandHandler(IUserSessionService userSessionService, ICheckDownloader checkDownloader, IExpenseCategorizer expenseCategorizer, IExpenseCategoryMappingCache expenseCategoryMappingCache, IMediator mediator) : IRequestHandler<DownloadExpenseFromFNSServiceCommand>
{
    public async Task Handle(DownloadExpenseFromFNSServiceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await mediator.Publish(new PreparingCategoryMappingStartedEvent(){SessionId = session.Id, }, cancellationToken);
            var mapping = await expenseCategoryMappingCache.Get(Currency.RUR, cancellationToken);
            
            var defaultCategory = Categories.Outcome.DefaultCategory;
            await mediator.Publish(new CategoryMappingPreparedEvent(){SessionId = session.Id}, cancellationToken);
        
            await mediator.Publish(new DownloadingExpenseStartedEvent(){SessionId = session.Id}, cancellationToken);
            var outcomes = await checkDownloader.DownloadExpenses(request.CheckRequisite, expenseCategorizer, mapping, defaultCategory);
            await mediator.Publish(new DownloadingExpenseFinishedEvent(){SessionId = session.Id}, cancellationToken);
            
            await mediator.Send(new SaveOutcomesBatchCommand() { SessionId = session.Id, MoneyTransfers = outcomes }, cancellationToken);
        }
    }
}