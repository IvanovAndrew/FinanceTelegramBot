using Application.AddExpense;
using Application.Events;
using Domain.Events;
using MediatR;

namespace Application.AddMoneyTransfer;

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand>
{
    private readonly IUserSessionService _userSessionService;
    private readonly IMediator _mediator;

    public CreateExpenseCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    {
        _userSessionService = userSessionService;
        _mediator = mediator;
    }
    
    public async Task Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(request.SessionID);

        if (session != null)
        {
            await _mediator.Publish(new OutcomeCreatedEvent() { SessionID = session.Id }, cancellationToken);
        }
    }
}