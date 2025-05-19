using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByMonth;

public class StatisticByMonthSaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string Date { get; init; } = string.Empty;
}

public class StatisticByMonthSaveDateCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator, ILogger<StatisticByMonthSaveDateCommandHandler> logger) : IRequestHandler<StatisticByMonthSaveDateCommand>
{
    public async Task Handle(StatisticByMonthSaveDateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"request.Date is {request.Date}");
        
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            logger.LogWarning($"A session {request.SessionId} has been found");
            
            if (dateTimeService.TryParseDate(request.Date, out var date))
            {
                var firstDayOfMonth = date.FirstDayOfMonth();
                session.StatisticsOptions.DateFrom = firstDayOfMonth;
                session.StatisticsOptions.DateTo = firstDayOfMonth.AddMonths(1).AddDays(-1);
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticByMonthSaveDateSavedEvent(){SessionId = session.Id}, cancellationToken);
            }
            else
            {
                await mediator.Publish(new CustomDateRequestedEvent()
                {
                    SessionId = session.Id,
                    LastSentMessageId = session.LastSentMessageId,
                    Text = $"Enter the day. Example: {dateTimeService.Today().ToString("MMMM yyyy")}",
                }, cancellationToken);
                session.LastSentMessageId = null;
            }
        }
        else
        {
            logger.LogWarning($"Couldn't find a session {request.SessionId}");
        }
    }
}