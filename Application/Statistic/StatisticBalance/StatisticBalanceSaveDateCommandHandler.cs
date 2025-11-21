using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceSaveDateCommandHandler(IUserSessionService sessionService, IDateTimeService dateTimeService, IMediator mediator, ILogger<StatisticBalanceSaveDateCommandHandler> logger) : IRequestHandler<StatisticBalanceSaveDateCommand>
{
    public async Task Handle(StatisticBalanceSaveDateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(StatisticBalanceSaveDateCommandHandler)} called {request}");
        
        var session = sessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (dateTimeService.TryParseDate(request.DateFrom, out var date))
            {
                session.StatisticsOptions.DateFrom = date;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticBalanceDateSavedEvent()
                    { SessionId = session.Id, LastSentMessageId = session.LastSentMessageId }, cancellationToken);
            }
            else
            {
                await mediator.Publish(new CustomDateRequestedEvent()
                {
                    SessionId = session.Id,
                    LastSentMessageId = session.LastSentMessageId,
                    Text = $"Enter the month. Example: {dateTimeService.Today().ToString("MMMM yyyy")}",
                }, cancellationToken);
                session.LastSentMessageId = null;
            }
        }
    }
}