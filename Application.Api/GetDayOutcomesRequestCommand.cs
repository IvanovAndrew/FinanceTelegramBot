using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public record GetDayOutcomesRequestCommand : IRequest<IReadOnlyList<Outcome>>
{
    public DateOnly Date { get; init; }
    public string Currency { get; init; }
}

public class GetDayOutcomesRequestCommandHandler(IFinanceRepository financeRepository, ILogger<GetDayOutcomesRequestCommandHandler> logger)
    : IRequestHandler<GetDayOutcomesRequestCommand, IReadOnlyList<Outcome>>
{
    public async Task<IReadOnlyList<Outcome>> Handle(GetDayOutcomesRequestCommand request,
        CancellationToken cancellationToken)
    {
        var currency = Currency.Parse(request.Currency);

        var query = new FinanceFilter
        {
            DateFrom = request.Date,
            DateTo = request.Date,
            Currency = currency
        };
        
        var outcomes = await financeRepository.ReadOutcomes(query, cancellationToken);
        return outcomes;
    }
}