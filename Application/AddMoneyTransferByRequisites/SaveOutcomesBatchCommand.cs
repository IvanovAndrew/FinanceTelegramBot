using Application.AddMoneyTransfer;
using Application.Events;
using Domain;
using MediatR;

namespace Application.Commands.SaveOutcomesBatch;

public class SaveOutcomesBatchCommand : IRequest
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
            
            var cancellationTokenSource = new CancellationTokenSource();
            session.CancellationTokenSource = cancellationTokenSource;

            try
            {
                using (cancellationTokenSource)
                {
                    bool success = await financeRepository.SaveAllOutcomes(request.MoneyTransfers, cancellationTokenSource.Token);

                    if (success)
                    {
                        result = SaveBatchExpensesResult.Saved(request.MoneyTransfers);
                    }
                    else
                    {
                        result = SaveBatchExpensesResult.Failed(request.MoneyTransfers, "Couldn't save expenses");
                    }
                }
            }
            catch (TaskCanceledException e)
            {
                result = SaveBatchExpensesResult.Canceled(request.MoneyTransfers);
            }

            await mediator.Publish(new ExpensesBatchSavedResultEvent() { SessionId = request.SessionId, Result = result }, cancellationToken);
        }
    }
}