using Application.Core;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot.Statistic;

public record GetStatisticMonthRequestCommand : IRequest<IReadOnlyList<Outcome>>
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class GetStatisticMonthRequestCommandHandler(IExpensesService expensesService, IMediator mediator, ILogger<GetStatisticMonthRequestCommandHandler> logger) : IRequestHandler<GetStatisticMonthRequestCommand, IReadOnlyList<Outcome>>
{
    public async Task<IReadOnlyList<Outcome>> Handle(GetStatisticMonthRequestCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(GetStatisticMonthRequestCommandHandler)} called {request}");

        var outcomes = await expensesService.GetAllExpenses(request.Query.Currency, request.Query.MonthRange.From.ToDateOnly(), request.Query.MonthRange.To.ToLastDayOfMonth(), cancellationToken);
        
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category?.Name ?? Categories.Outcome.DefaultCategory.Name, sortByMoney: true, sortAsc: false);
        var statistic = expenseAggregator.Aggregate(outcomes.ToList(), [request.Query.Currency]);
        var wrapper = StatisticMapper.Map(statistic, new StringColumnFactory());

        await mediator.Publish(new StatisticOutcomesReadEvent()
        {
            SessionId = request.SessionId,
            Statistic = wrapper,

            Subtitle = $"Expenses for {request.Query.MonthRange.From.ToString(DateFormat.FullMonthName)}",
            FirstColumnName = "Category",
        }, cancellationToken);

        return outcomes;
    }
}