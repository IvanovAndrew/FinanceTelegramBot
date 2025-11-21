using Application.Events;
using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record StatisticByCategorySaveDateCommand : IRequest
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
                session.StatisticsOptions.DateFrom = date.FirstDayOfMonth();
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticByCategorySaveDateSavedEvent() { SessionId = session.Id}, cancellationToken);
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