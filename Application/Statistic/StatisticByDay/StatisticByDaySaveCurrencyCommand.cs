using Domain;
using MediatR;

namespace Application.Commands.StatisticByDay;

public class StatisticByDaySaveCurrencyCommand : IRequest
{
    public long SessionId { get; init; }
    public string Currency { get; init; }
}

public class StatisticByDayCurrencyCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<StatisticByDaySaveCurrencyCommand>
{
    public async Task Handle(StatisticByDaySaveCurrencyCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (Currency.TryParse(request.Currency, out var currency))
            {
                session.StatisticsOptions.Currency = currency;
                session.StatisticsOptions.CurrencySpecified = true;
            }
            else if (string.Equals(request.Currency, "All"))
            {
                session.StatisticsOptions.CurrencySpecified = true;
            }

            if (session.StatisticsOptions.CurrencySpecified)
            {
                await mediator.Send(new StatisticDayRequestCommand() { SessionId = request.SessionId },
                    cancellationToken);
            }
        }
    }
}