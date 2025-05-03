using Application.Events;
using MediatR;

namespace Application.Commands;

public class StartSessionCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StartSessionCommand>
{
    public async Task Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var session = new UserSession() { Id = request.SessionId };
        userSessionService.SaveUserSession(session);

        await mediator.Publish(new UserStartedEvent() { SessionID = request.SessionId }, cancellationToken);
    }
}