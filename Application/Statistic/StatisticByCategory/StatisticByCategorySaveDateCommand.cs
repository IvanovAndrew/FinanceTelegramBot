using Application.Events;
using Domain.Events;
using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategorySaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateText { get; init; } = string.Empty;
}

public class StatisticByCategorySaveDateCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) : IRequestHandler<StatisticByCategorySaveDateCommand>
{
    public async Task Handle(StatisticByCategorySaveDateCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (dateTimeService.TryParseDate(request.DateText, out var date))
            {
                var firstDayOfMonth = new DateOnly(date.Year, date.Month, 1);
                session.StatisticsOptions.DateFrom = firstDayOfMonth;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticByCategorySaveDateSavedEvent() { SessionId = session.Id}, cancellationToken);
            }
            else
            {
                session.LastSentMessageId = null;
                await mediator.Publish(new CustomDateRequestedEvent()
                {
                    SessionId = session.Id,
                    Text = $"Enter the month. Example: {dateTimeService.Today().ToString("MMMM yyyy")}",
                }, cancellationToken);
            }
        }
    }
}