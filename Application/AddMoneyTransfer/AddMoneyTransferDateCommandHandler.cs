using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferDateCommandHandler(
    IMediator mediator,
    IUserSessionService userSessionService,
    IDateTimeService dateTimeService, ILogger<AddMoneyTransferDateCommandHandler> logger)
    : IRequestHandler<AddMoneyTransferDateCommand>
{
    public async Task Handle(AddMoneyTransferDateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AddMoneyTransferDateCommandHandler)}.Handle({request})");
        
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var builder = session.MoneyTransferBuilder;

            if (dateTimeService.TryParseDate(request.DateText, out var date))
            {
                builder.Date = date;
                session.QuestionnaireService.Next();
                await mediator.Publish(new MoneyTransferDateEnteredEvent() { SessionId = request.SessionId },
                    cancellationToken);
            }
            else
            {
                logger.LogWarning($"Couldn't parse {request.DateText} as a date");
                await mediator.Publish(new CustomDateChosenEvent() { SessionId = request.SessionId },
                    cancellationToken);
            }
        }
        
        logger.LogInformation($"{nameof(AddMoneyTransferDateCommandHandler)} finished)");
    }
}