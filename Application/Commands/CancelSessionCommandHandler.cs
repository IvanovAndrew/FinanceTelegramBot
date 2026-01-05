using MediatR;

namespace Application.Commands;

public class CancelSessionCommandHandler(IUserSessionService userSessionService) : IRequestHandler<CancelSessionCommand>
{
    public async Task Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            userSessionService.RemoveSession(session);
        }
    }
}