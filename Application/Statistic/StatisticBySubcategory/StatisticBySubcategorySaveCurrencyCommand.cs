using Application.Commands.StatisticBySubcategoryByMonth;
using Domain;
using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticBySubcategorySaveCurrencyCommand : IRequest
{
    public long SessionId { get; init; }
    public string Currency { get; init; }
}

public class StatisticBySubcategoryMonthSaveCurrencyCommand : IRequest
{
    public long SessionId { get; init; }
    public string Currency { get; init; }
}

public class StatisticBySubcategoryMonthSaveCurrencyCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticBySubcategoryMonthSaveCurrencyCommand>
{
    public async Task Handle(StatisticBySubcategoryMonthSaveCurrencyCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (Currency.TryParse(request.Currency, out var currency))
            {
                session.StatisticsOptions.Currency = currency;
                session.StatisticsOptions.CurrencySpecified = true;
            }
            else if (string.Equals(request.Currency, "All", StringComparison.InvariantCultureIgnoreCase))
            {
                session.StatisticsOptions.CurrencySpecified = true;
            }

            if (session.StatisticsOptions.CurrencySpecified)
            {
                await mediator.Send(new StatisticSubcategoryMonthRequestCommand() { SessionId = request.SessionId }, cancellationToken);
            }
        }
    }
}