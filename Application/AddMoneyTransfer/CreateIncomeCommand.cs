using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public record CreateIncomeCommand : IRequest
{
    public long SessionID { get; init; }
}

public class CreateIncomeCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator, ILogger<CreateIncomeCommandHandler> logger)
    : IRequestHandler<CreateIncomeCommand>
{
    public async Task Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionID);

        if (session != null)
        {
            session.ActiveFlow = new AddMoneyTransferFlow(true, dateTimeService, logger);
            await mediator.Publish(new IncomeCreatedEvent() { SessionId = session.Id, LastSentMessageId = session.LastSentMessageId }, cancellationToken);
        }
    }
}