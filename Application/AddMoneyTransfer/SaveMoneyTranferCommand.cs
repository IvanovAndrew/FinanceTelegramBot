using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public record SaveMoneyTransferCommand : IRequest
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public IMoneyTransfer MoneyTransfer { get; init; }
    
}

public class SaveMoneyTransferCommandHandler(IFinanceRepository financeRepository, IMediator mediator)
    : IRequestHandler<SaveMoneyTransferCommand>
{
    public async Task Handle(SaveMoneyTransferCommand request, CancellationToken cancellationToken)
    {
        await mediator.Publish(
            new MoneyTransferSavingStartedEvent()
                { SessionId = request.SessionId, MessageId = request.LastSentMessageId }, cancellationToken);
                
        SaveResult success = SaveResult.Fail("Unknown money transfer type");
        
        try
        {
            success = await financeRepository.Save(request.MoneyTransfer, cancellationToken);
        }
        catch (TaskCanceledException e)
        {
            // TODO send an event 'task canceled'
            return;
        }
        
        if (success.Success)
        {
            await mediator.Publish(new MoneyTransferSavedEvent { SessionId = request.SessionId, MoneyTransfer = request.MoneyTransfer }, cancellationToken);
        }
        else
        {
            await mediator.Publish(new MoneyTransferIsNotSavedEvent { SessionId = request.SessionId, Reason = success.ErrorMessage!},
                cancellationToken);
        }
    }
}