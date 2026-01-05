using Application.AddMoneyTransfer;
using Domain;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record SaveOutcomesBatchCommand : IRequest
{
    public long SessionId { get; init; }
    public IReadOnlyCollection<IMoneyTransfer> MoneyTransfers { get; init; }
}

public class SaveOutcomesBatchCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<SaveOutcomesBatchCommand>
{
    public async Task Handle(SaveOutcomesBatchCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            SaveBatchExpensesResult result;

            await mediator.Publish(new MoneyTransferSavingStartedEvent()
                { SessionId = session.Id, MessageId = (int)session.LastSentMessageId! }, cancellationToken);
            
            try
            {
                var success = await financeRepository.SaveAllOutcomes(request.MoneyTransfers, cancellationToken);

                result = success.Success ? SaveBatchExpensesResult.Saved(request.MoneyTransfers) : SaveBatchExpensesResult.Failed(request.MoneyTransfers, success.ErrorMessage!);
            }
            catch (TaskCanceledException e)
            {
                result = SaveBatchExpensesResult.Canceled(request.MoneyTransfers);
            }

            await mediator.Publish(new ExpensesBatchSavedResultEvent() { SessionId = request.SessionId, Result = result }, cancellationToken);
        }
    }
}