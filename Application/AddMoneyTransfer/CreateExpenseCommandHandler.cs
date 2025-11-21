using MediatR;

namespace Application.AddMoneyTransfer;

public class CreateExpenseCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<CreateExpenseCommand>
{
    public async Task Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionID);

        if (session != null)
        {
            await mediator.Publish(new OutcomeCreatedEvent() { SessionID = session.Id }, cancellationToken);
        }
    }
}