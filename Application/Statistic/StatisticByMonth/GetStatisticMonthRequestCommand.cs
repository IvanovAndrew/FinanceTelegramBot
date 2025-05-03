using Domain;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.StatisticByMonth;

public class GetStatisticMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
}

public class GetStatisticMonthRequestCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator, ILogger<GetStatisticMonthRequestCommandHandler> logger) : IRequestHandler<GetStatisticMonthRequestCommand>
{
    public async Task Handle(GetStatisticMonthRequestCommand dayRequest, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(GetStatisticMonthRequestCommandHandler)} called");
        
        var session = userSessionService.GetUserSession(dayRequest.SessionId);
        if (session != null)
        {
            var sessionStatisticsOptions = session.StatisticsOptions;
            
            var filter = new FinanceFilter()
            {
                DateFrom = sessionStatisticsOptions.DateFrom,
                DateTo = sessionStatisticsOptions.DateTo,
                Currency = sessionStatisticsOptions.Currency,
            };
            
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc: false);

            session.CancellationTokenSource = new CancellationTokenSource();
            
            using (session.CancellationTokenSource)
            {
                var outcomes = await financeRepository.ReadOutcomes(filter, session.CancellationTokenSource.Token);

                if (outcomes.Any())
                {
                    var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
                    var statistic = expenseAggregator.Aggregate(outcomes, currencies);
                    
                    await mediator.Publish(new MoneyTransferReadDomainEvent<string>()
                        { 
                            SessionId = session.Id, 
                            Statistic = statistic,
                            Subtitle = $"Expenses for {sessionStatisticsOptions.DateTo.Value.ToString("MMMM yyyy")}",
                            FirstColumnName = "Category",
                            DateFrom = filter.DateFrom, 
                            DateTo = filter.DateTo,
                        }, cancellationToken);
                }
            }
        }
    }
}