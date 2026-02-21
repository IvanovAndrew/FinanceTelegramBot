using Application.Events;
using MediatR;

namespace Application.Statistic.StatisticByDay;

public record StatisticSaveDayCommand : IRequest
{
    public long SessionId { get; init; }
    public string Date { get; init; }
}

public class StatisticSaveDayCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) 
    : IRequestHandler<StatisticSaveDayCommand>
{
    public async Task Handle(StatisticSaveDayCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session?.ActiveFlow is not StatisticsFlow flow)
            return;

        var draft = flow.Draft;
        if (dateTimeService.TryParseDate(request.Date, out var date))
        {
            draft.Day = date;

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