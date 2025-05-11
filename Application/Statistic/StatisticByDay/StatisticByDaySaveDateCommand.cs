using Application.Events;
using Domain.Events;
using MediatR;

namespace Application.Statistic.StatisticByDay;

public class StatisticByDaySaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string Date { get; init; }
}

public class StatisticByDaySaveDateCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) 
    : IRequestHandler<StatisticByDaySaveDateCommand>
{
    public async Task Handle(StatisticByDaySaveDateCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (dateTimeService.TryParseDate(request.Date, out var date))
            {
                session.StatisticsOptions.DateFrom = date;
                session.StatisticsOptions.DateTo = date;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticByDayDateSavedEvent() { SessionId = session.Id }, cancellationToken);
            }
            else
            {
                await mediator.Publish(new CustomDateRequestedEvent()
                {
                    SessionId = session.Id,
                    LastSentMessageId = session.LastSentMessageId,
                    Text = $"Enter the day. Example: {dateTimeService.Today().ToString("d MMMM yyyy")}",
                }, cancellationToken);
                session.LastSentMessageId = null;
            }
        }
    }
}