using Application.Core;
using Application.Core.Commands;
using MediatR;

namespace Application.Bot;

public class CancelSessionCommandHandler(IUserSessionService userSessionService, IConversation conversation) : IRequestHandler<CancelSessionCommand>
{
    public async Task Handle(CancelSessionCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            userSessionService.RemoveSession(session.Id);
        }

		await conversation.Update(request.SessionId, Screens.Canceled(), cancellationToken);
    }
}