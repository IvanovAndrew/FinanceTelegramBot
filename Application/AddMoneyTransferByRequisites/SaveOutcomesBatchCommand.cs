using Domain;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record SaveOutcomesBatchCommand : IRequest
{
    public long SessionId { get; init; }
    public IReadOnlyCollection<Outcome> MoneyTransfers { get; init; }
}

public class SaveOutcomesBatchCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IProgressNotifier progressNotifier) : IRequestHandler<SaveOutcomesBatchCommand>
{
    public async Task Handle(SaveOutcomesBatchCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            SaveBatchExpensesResult result;

            await progressNotifier.Start(request.SessionId, "Saving...", cancellationToken);
            
            try
            {
                var success = await financeRepository.SaveAllOutcomes(request.MoneyTransfers, cancellationToken);

                result = success.Success ? SaveBatchExpensesResult.Saved(request.MoneyTransfers) : SaveBatchExpensesResult.Failed(request.MoneyTransfers, success.ErrorMessage!);
            }
            catch (TaskCanceledException e)
            {
                result = SaveBatchExpensesResult.Canceled(request.MoneyTransfers);
            }

            await progressNotifier.Finish(request.SessionId, result.GetMessage(), cancellationToken);
        }
    }
}