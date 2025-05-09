using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceSaveDateCommandHandler(IUserSessionService sessionService, IDateTimeService dateTimeService, IMediator mediator, ILogger<StatisticBalanceSaveDateCommandHandler> logger) : IRequestHandler<StatisticBalanceSaveDateCommand>
{
    public async Task Handle(StatisticBalanceSaveDateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"StatisticBalanceSaveDateCommandHandler called");
        
        var session = sessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (dateTimeService.TryParseDate(request.DateFrom, out var date))
            {
                session.StatisticsOptions.DateFrom = date;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticBalanceDateSaved()
                    { SessionId = session.Id, LastSentMessageId = (int)session.LastSentMessageId! }, cancellationToken);
            }
        }
    }
}