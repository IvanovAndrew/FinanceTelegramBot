using Application.Core.AddMoneyTransfer;
using MediatR;

namespace Application.Bot;

public class CreateExpenseCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<CreateExpenseCommand>
{
    public async Task Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await mediator.Publish(new OutcomeCreatedEvent() { SessionId = session.Id }, cancellationToken);
        }
    }
}