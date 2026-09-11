using Application.Core;
using Application.Core.Commands;
using MediatR;

namespace Application.Bot;

public class StartSessionCommandHandler(IUserSessionService userSessionService, IConversation conversation) : IRequestHandler<StartSessionCommand>
{
    public async Task Handle(StartSessionCommand request, CancellationToken cancellationToken)
    {
        var session = new UserSession() { Id = request.SessionId };
        userSessionService.SaveUserSession(session);

        // if we had
        await conversation.Finish(request.SessionId, cancellationToken);
        
        await conversation.Update(request.SessionId, Screens.SelectStart(), cancellationToken);
    }
}