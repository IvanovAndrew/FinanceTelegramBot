using Application.Core;
using Application.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot;

public record CreateIncomeCommand : IRequest
{
    public long SessionId { get; init; }
}

public class CreateIncomeCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator, ILogger<CreateIncomeCommandHandler> logger)
    : IRequestHandler<CreateIncomeCommand>
{
    public async Task Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.ActiveFlow = new AddMoneyTransferFlow(true, dateTimeService, logger);
            await mediator.Publish(new DraftUpdatedEvent { SessionId = request.SessionId }, cancellationToken);
        }
    }
}