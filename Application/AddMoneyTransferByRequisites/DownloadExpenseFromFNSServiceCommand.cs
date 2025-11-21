using Application.Contracts;
using Application.Events;
using Domain;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record DownloadExpenseFromFNSServiceCommand : IRequest
{
    public long SessionId { get; init; }
    public CheckRequisite CheckRequisite { get; init; }
}

public class DownloadExpenseFromFNSServiceCommandHandler(IUserSessionService userSessionService, ICheckDownloader checkDownloader, IExpenseCategorizer expenseCategorizer, ICategoryProvider categoryProvider, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<DownloadExpenseFromFNSServiceCommand>
{
    public async Task Handle(DownloadExpenseFromFNSServiceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await mediator.Publish(new DownloadingExpenseStartedEvent(){SessionId = session.Id}, cancellationToken);
            
            var cancellationTokenSource = new CancellationTokenSource();
            session.CancellationTokenSource = cancellationTokenSource;

            using (cancellationTokenSource)
            {
                var getAllOutcomes = financeRepository.ReadOutcomes(new FinanceFilter() { Currency = checkDownloader.Currency }, cancellationToken);
                
                session.QuestionnaireService = null;
            
                await mediator.Publish(new DownloadingExpenseFinishedEvent(){SessionId = session.Id}, cancellationToken);

                var defaultCategory = categoryProvider.DefaultOutcomeCategory();

                var knownOutcomes = await getAllOutcomes;
                var dict = knownOutcomes.DistinctBy(t => t.Description).ToDictionary(t => t.Description,
                    t => ExpenseCategorizerResult.Create(t.Category, t.SubCategory));
            
                var outcomes = await checkDownloader.DownloadExpenses(request.CheckRequisite, expenseCategorizer, dict, defaultCategory);
                await mediator.Send(new SaveOutcomesBatchCommand() { SessionId = session.Id, MoneyTransfers = outcomes }, cancellationToken);
            }
        }
    }
}