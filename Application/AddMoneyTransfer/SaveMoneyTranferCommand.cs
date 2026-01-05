using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public record SaveMoneyTransferCommand : IRequest
{
    public long SessionId { get; init; }
}

public class SaveMoneyTransferCommandHandler : IRequestHandler<SaveMoneyTransferCommand>
{
    private readonly IUserSessionService _userSessionService;
    private readonly IFinanceRepository _financeRepository;
    private readonly IMediator _mediator;

    public SaveMoneyTransferCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator)
    {
        _userSessionService = userSessionService;
        _financeRepository = financeRepository;
        _mediator = mediator;
    }
    
    public async Task Handle(SaveMoneyTransferCommand request, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var moneyTransfer = session.MoneyTransferBuilder.Build();

            await _mediator.Publish(
                new MoneyTransferSavingStartedEvent()
                    { SessionId = session.Id, MessageId = (int)session.LastSentMessageId! }, cancellationToken);
                
            try
            {
                SaveResult success;

                if (moneyTransfer.IsIncome)
                {
                    success = await _financeRepository.SaveIncome(moneyTransfer, cancellationToken);
                }
                else
                {
                    success = await _financeRepository.SaveAllOutcomes(new List<IMoneyTransfer>() { moneyTransfer }, cancellationToken);
                }

                if (success.Success)
                {
                    await _mediator.Publish(new MoneyTransferSavedEvent { SessionId = request.SessionId, MoneyTransfer = moneyTransfer }, cancellationToken);
                }
                else
                {
                    await _mediator.Publish(new MoneyTransferIsNotSavedEvent { SessionId = request.SessionId, Reason = success.ErrorMessage!},
                        cancellationToken);
                }
            }
            catch (TaskCanceledException e)
            {
                // TODO send an event 'task canceled' 
            }
        }
    }
}