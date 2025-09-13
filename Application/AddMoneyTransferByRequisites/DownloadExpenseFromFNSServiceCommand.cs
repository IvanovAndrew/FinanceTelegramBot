using Application.Contracts;
using Application.Events;
using Domain;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class DownloadExpenseFromFNSServiceCommand : IRequest
{
    public long SessionId { get; init; }
    public CheckRequisite CheckRequisite { get; init; }
}

public class DownloadExpenseFromFNSServiceCommandHandler(IUserSessionService userSessionService, IFnsService fnsService, IExpenseCategorizer expenseCategorizer, ICategoryProvider categoryProvider, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<DownloadExpenseFromFNSServiceCommand>
{
    public async Task Handle(DownloadExpenseFromFNSServiceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await mediator.Publish(new DownloadingExpenseStartedEvent(){SessionId = session.Id}, cancellationToken);
            
            var cancellationTokenSource = new CancellationTokenSource();
            session.CancellationTokenSource = cancellationTokenSource;

            IReadOnlyCollection<RawOutcomeItem> rawOutcomeItems = new List<RawOutcomeItem>();
            List<Outcome> outcomes = new List<Outcome>();
            
            using (cancellationTokenSource)
            {
                var getFnsCheckTask = fnsService.GetCheck(request.CheckRequisite);
                var getAllOutcomes = financeRepository.ReadOutcomes(new FinanceFilter() { Currency = Currency.Rur }, cancellationToken);
                
                rawOutcomeItems = await getFnsCheckTask; 
                
                session.QuestionnaireService = null;
            
                await mediator.Publish(new DownloadingExpenseFinishedEvent(){SessionId = session.Id}, cancellationToken);

                var defaultCategory = categoryProvider.DefaultOutcomeCategory();

                var knownOutcomes = await getAllOutcomes;
                var dict = knownOutcomes.DistinctBy(t => t.Description).ToDictionary(t => t.Description,
                    t => ExpenseCategorizerResult.Create(t.Category, t.SubCategory));
            
                foreach (var rawOutcome in rawOutcomeItems)
                {
                    var expenseCategoryResult = expenseCategorizer.GetCategory(rawOutcome.Description, dict);

                    outcomes.Add(
                        new Outcome()
                        {
                            Date = rawOutcome.Date,
                            Category = expenseCategoryResult?.Category?? defaultCategory,
                            SubCategory = expenseCategoryResult?.SubCategory,
                            Description = rawOutcome.Description,
                            
                            Amount = new Money(){Amount = rawOutcome.Amount, Currency = Currency.Rur},
                        });
                }
            }

            await mediator.Send(new SaveOutcomesBatchCommand() { SessionId = session.Id, MoneyTransfers = outcomes }, cancellationToken);
        }
    }
}