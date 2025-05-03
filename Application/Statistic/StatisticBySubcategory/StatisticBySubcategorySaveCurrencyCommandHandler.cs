using Application.Commands.StatisticBySubcategory;
using Domain;
using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticBySubcategorySaveCurrencyCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticBySubcategorySaveCurrencyCommand>
{
    public async Task Handle(StatisticBySubcategorySaveCurrencyCommand request, CancellationToken cancellationToken)
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
                await mediator.Send(new StatisticSubcategoryRequestCommand() { SessionId = request.SessionId }, cancellationToken);
            }
        }
    }
}