using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public record SaveMoneyTransferCommand : IRequest
{
    public long SessionId { get; init; }
    public IMoneyTransfer MoneyTransfer { get; init; }
}

public class SaveMoneyTransferCommandHandler(IFinanceRepository financeRepository, IProgressNotifier progressNotifier)
    : IRequestHandler<SaveMoneyTransferCommand>
{
    public async Task Handle(SaveMoneyTransferCommand request, CancellationToken cancellationToken)
    {
        await progressNotifier.Start(request.SessionId, "Saving...", cancellationToken);
                
        SaveResult success;
        
        try
        {
            success = await financeRepository.Save(request.MoneyTransfer, cancellationToken);
        }
        catch (TaskCanceledException e)
        {
            await progressNotifier.Finish(request.SessionId, "Saving cancelled", cancellationToken);
            return;
        }
        
        if (!success.Success)
        {
            await progressNotifier.Finish(request.SessionId, $"Couldn't save expense. {success.ErrorMessage}", cancellationToken);
            return;
        }
            
        await progressNotifier.Finish(
            request.SessionId, 
            string.Join($"{Environment.NewLine}", request.MoneyTransfer.ToString(), string.Empty, "Saved"), 
            cancellationToken);
    }
}