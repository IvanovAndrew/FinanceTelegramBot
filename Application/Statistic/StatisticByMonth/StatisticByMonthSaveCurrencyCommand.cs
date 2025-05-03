using Domain;
using MediatR;

namespace Application.Commands.StatisticByMonth;

public class StatisticByMonthSaveCurrencyCommand : IRequest
{
    public long SessionId { get; init; }
    public string Currency { get; init; }
}

public class StatisticByMonthSaveCurrencyCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticByMonthSaveCurrencyCommand>
{
    public async Task Handle(StatisticByMonthSaveCurrencyCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (Currency.TryParse(request.Currency, out var currency))
            {
                session.StatisticsOptions.Currency = currency;
                session.StatisticsOptions.CurrencySpecified = true;
            }
            else if (string.Equals(request.Currency, "ALL", StringComparison.InvariantCultureIgnoreCase))
            {
                session.StatisticsOptions.CurrencySpecified = true;
            }

            if (session.StatisticsOptions.CurrencySpecified)
            {
                session.QuestionnaireService.Next();
                await mediator.Send(new GetStatisticMonthRequestCommand(){SessionId = session.Id});
            }
        }
    }
}