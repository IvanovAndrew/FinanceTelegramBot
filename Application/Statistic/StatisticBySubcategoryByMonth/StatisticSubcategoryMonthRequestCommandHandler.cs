using Application.AddMoneyTransfer;
using Application.Statistic.StatisticBySubcategoryByMonth;
using Domain;
using MediatR;

namespace Application.Commands.StatisticBySubcategoryByMonth;

public class StatisticSubcategoryMonthRequestCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<StatisticSubcategoryMonthRequestCommand>
{
    public async Task Handle(StatisticSubcategoryMonthRequestCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);
        if (session != null)
        {
            var sessionStatisticsOptions = session.StatisticsOptions;
            
            var filter = new FinanceFilter()
            {
                DateFrom = sessionStatisticsOptions.DateFrom,
                DateTo = sessionStatisticsOptions.DateTo,
                Category = session.StatisticsOptions.Category,
                Subcategory = session.StatisticsOptions.Subcategory,
                Currency = sessionStatisticsOptions.Currency,
            };
            
            var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date.FirstDayOfMonth(), false, sortAsc: true);

            session.CancellationTokenSource = new CancellationTokenSource();
            
            using (session.CancellationTokenSource)
            {
                var outcomes = await financeRepository.ReadOutcomes(filter, session.CancellationTokenSource.Token);

                if (outcomes.Any())
                {
                    var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
                    var statistic = expenseAggregator.Aggregate(outcomes, currencies);

                    var subtitle = $"Category: {filter.Category?.Name}{Environment.NewLine}" +
                                   $"Subcategory: {filter.Subcategory?.Name}{Environment.NewLine}" +
                                   $"Expenses from {filter.DateFrom.Value.ToString("MMMM yyyy")}";
                    
                    await mediator.Publish(new MoneyTransferReadDomainEvent()
                    { 
                        SessionId = session.Id, 
                        Statistic = StatisticMapper.Map(statistic, new DateOnlyColumnFactory()), 
                        Subtitle = subtitle,
                        FirstColumnName = "Month",
                    }, cancellationToken);
                }
            }
        }
    }
}