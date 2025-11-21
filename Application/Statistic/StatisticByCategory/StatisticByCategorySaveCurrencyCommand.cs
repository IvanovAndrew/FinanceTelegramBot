using Domain;
using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record StatisticByCategorySaveCurrencyCommand : IRequest
{
    public long SessionId { get; init; }
    public string Currency { get; init; }
}

public class StatisticByCategorySaveCurrencyCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<StatisticByCategorySaveCurrencyCommand>
{
    public async Task Handle(StatisticByCategorySaveCurrencyCommand request, CancellationToken cancellationToken)
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

            await mediator.Send(new GetStatisticCategoryRequestCommand() { SessionId = session.Id }, cancellationToken);
        }
    }
}