using Application.Core;
using Domain;
using MediatR;

namespace Application.Bot;

public record SaveOutcomesBatchCommand : IRequest
{
    public long SessionId { get; init; }
    public IReadOnlyCollection<Outcome> MoneyTransfers { get; init; }
    public string FileName { get; init; }
}

public class SaveOutcomesBatchCommandHandler(IFinanceRepository financeRepository, IConversation conversation) : IRequestHandler<SaveOutcomesBatchCommand>
{
    public async Task Handle(SaveOutcomesBatchCommand request, CancellationToken cancellationToken)
    {
        SaveBatchExpensesResult result;

        await conversation.Update(request.SessionId,  Screens.NotifyOperationInProgress($"Saving expenses from {request.FileName}..."), cancellationToken);
        
        try
        {
            var success = await financeRepository.SaveAllOutcomes(request.MoneyTransfers, cancellationToken);

            result = success.Success ? SaveBatchExpensesResult.Saved(request.MoneyTransfers, request.FileName) : SaveBatchExpensesResult.Failed(request.MoneyTransfers, success.ErrorMessage!);
        }
        catch (TaskCanceledException e)
        {
            result = SaveBatchExpensesResult.Canceled(request.MoneyTransfers);
        }

        await conversation.Update(request.SessionId, Screens.Notify(result.GetMessage()), cancellationToken);
    }
}