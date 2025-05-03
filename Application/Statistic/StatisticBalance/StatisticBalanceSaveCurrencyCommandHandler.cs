using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceSaveCurrencyCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticBalanceSaveCurrencyCommand>
{
    public async Task Handle(StatisticBalanceSaveCurrencyCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (Currency.TryParse(request.Currency, out var currency))
            {
                session.StatisticsOptions.Currency = currency;
                session.QuestionnaireService.Next();
                await mediator.Send(new GetBalanceStatisticCommand(){SessionId = session.Id, LastSentMessage = (int) session.LastSentMessageId!}, cancellationToken);
            }
        }
    }
}