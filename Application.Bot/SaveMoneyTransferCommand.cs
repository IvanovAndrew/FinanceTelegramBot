using Application.Core;
using Domain;
using MediatR;

namespace Application.Bot;

public record SaveMoneyTransferCommand : IRequest
{
    public long SessionId { get; init; }
    public IMoneyTransfer MoneyTransfer { get; init; }
}

public class SaveMoneyTransferCommandHandler(IFinanceRepository financeRepository, IConversation chat)
    : IRequestHandler<SaveMoneyTransferCommand>
{
    public async Task Handle(SaveMoneyTransferCommand request, CancellationToken cancellationToken)
    {
        await chat.Update(request.SessionId, Screens.NotifyOperationInProgress("Saving..."), cancellationToken);
                
        SaveResult result;
        
        try
        {
            result = await financeRepository.Save(request.MoneyTransfer, cancellationToken);
        }
        catch (TaskCanceledException e)
        {
            await chat.Update(request.SessionId, Screens.Canceled(), cancellationToken);
            return;
        }
        
        if (!result.Success)
        {
            await chat.Update(request.SessionId, Screens.Notify($"Couldn't save expense. {result.ErrorMessage}"), cancellationToken);
            return;
        }
            
        await chat.Update(
            request.SessionId, 
            Screens.Notify(string.Join($"{Environment.NewLine}", request.MoneyTransfer.ToString(), string.Empty, "Saved")), 
            cancellationToken);
    }
}