using Application.Core;
using Domain;
using Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public record GetStatisticMonthApiRequestCommand : IRequest<Dictionary<Category, Money>>
{
    public DateOnly Month { get; init; }
    public string Currency { get; init; }
}

public class GetStatisticMonthApiRequestCommandHandler(IExpensesService expensesService, IMediator mediator, ILogger<GetStatisticMonthApiRequestCommandHandler> logger) : IRequestHandler<GetStatisticMonthApiRequestCommand, Dictionary<Category, Money>>
{
    public async Task<Dictionary<Category, Money>> Handle(GetStatisticMonthApiRequestCommand request, CancellationToken cancellationToken)
    {
        var currency = Currency.Parse(request.Currency);
        
        var outcomes = await expensesService.GetAllExpenses(currency, request.Month.FirstDayOfMonth(), request.Month.LastDayOfMonth(), cancellationToken);
        
        var aggregator = new ExpensesAggregator<Category>(x => x.Category, true, false);
        var statistic = aggregator.Aggregate(outcomes, [currency]);
        
        return statistic.Rows.ToDictionary(x => x.Row, x => x[currency]);
    }
}
