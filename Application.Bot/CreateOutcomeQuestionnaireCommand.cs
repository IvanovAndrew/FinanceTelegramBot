using Application.Core;
using Application.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot;

public record CreateOutcomeQuestionnaireCommand : IRequest
{
    public long SessionId { get; init; }
}

public class CreateOutcomeQuestionnaireCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator, ILogger<CreateOutcomeQuestionnaireCommand> logger) : IRequestHandler<CreateOutcomeQuestionnaireCommand>
{
    public async Task Handle(CreateOutcomeQuestionnaireCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.ActiveFlow = new AddMoneyTransferFlow(false, dateTimeService, logger);
            await mediator.Publish(new DraftUpdatedEvent { SessionId = request.SessionId }, cancellationToken);
        }
    }
}