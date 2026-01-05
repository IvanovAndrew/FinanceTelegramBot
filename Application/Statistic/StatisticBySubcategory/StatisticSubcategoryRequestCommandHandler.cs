using Application.AddMoneyTransfer;
using Domain;
using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticSubcategoryRequestCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<StatisticSubcategoryRequestCommand>
{
    public async Task Handle(StatisticSubcategoryRequestCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);
        if (session != null)
        {
            var sessionStatisticsOptions = session.StatisticsOptions;
            
            var filter = new FinanceFilter()
            {
                DateFrom = sessionStatisticsOptions.DateFrom,
                Category = session.StatisticsOptions.Category,
                Currency = sessionStatisticsOptions.Currency,
            };
            
            var expenseAggregator = new ExpensesAggregator<string>(e => e.SubCategory!.Name, true, sortAsc: false);

            var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);

            if (outcomes.Any())
            {
                var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
                var statistic = expenseAggregator.Aggregate(outcomes, currencies);
                
                await mediator.Publish(new MoneyTransferReadDomainEvent()
                    { 
                        SessionId = session.Id, 
                        Statistic = StatisticMapper.Map(statistic, new StringColumnFactory()),
                        Subtitle = $"Category: {filter.Category?.Name}{Environment.NewLine}Expenses from {filter.DateFrom.Value.ToString("MMMM yyyy")}",
                        FirstColumnName = "Subcategory",
                    }, 
                    cancellationToken);
            }
        }
    }
}