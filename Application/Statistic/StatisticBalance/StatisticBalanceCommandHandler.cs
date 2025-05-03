using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticBalanceCommand>
{
    public async Task Handle(StatisticBalanceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.QuestionnaireService = new StatisticBalanceQuestionnaire();
            session.StatisticsOptions = new StatisticsOptions();

            await mediator.Publish(new StatisticBalanceQuestionnaireCreated() { SessionId = session.Id, LastSentMessageId = (int) session.LastSentMessageId!}, cancellationToken);
        }
    }
}