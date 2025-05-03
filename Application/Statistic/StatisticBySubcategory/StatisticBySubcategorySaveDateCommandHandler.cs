using Application.Events;
using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticBySubcategorySaveDateCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator) : IRequestHandler<StatisticBySubcategorySaveDateCommand>
{
    public async Task Handle(StatisticBySubcategorySaveDateCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (dateTimeService.TryParseDate(request.DateFromText, out var date))
            {
                session.StatisticsOptions.DateFrom = date;
                session.QuestionnaireService.Next();

                await mediator.Publish(new StatisticBySubcategoryDateFromSavedEvent() { SessionId = session.Id }, cancellationToken);
            }
            else
            {
                session.LastSentMessageId = null;
                await mediator.Publish(new CustomDateRequestedEvent() { SessionId = session.Id, Text = $"Choose start of the period. Example: {dateTimeService.Today().ToString("MMMM yyyy")}"}, cancellationToken);
            }
        }
    }
}