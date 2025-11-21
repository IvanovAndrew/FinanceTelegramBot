using Application.AddMoneyTransfer;
using Domain;
using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record GetStatisticCategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
}

public class GetStatisticCategoryRequestCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<GetStatisticCategoryRequestCommand>
{
    public async Task Handle(GetStatisticCategoryRequestCommand request, CancellationToken cancellationToken)
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
        
            var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date.FirstDayOfMonth(), false, sortAsc: true);

            session.CancellationTokenSource = new CancellationTokenSource();
        
            using (session.CancellationTokenSource)
            {
                var outcomes = await financeRepository.ReadOutcomes(filter, session.CancellationTokenSource.Token);

                if (outcomes.Any())
                {
                    var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
                    var statistic = expenseAggregator.Aggregate(outcomes, currencies);
                
                    await mediator.Publish(new MoneyTransferReadDomainEvent()
                        { 
                            SessionId = session.Id, 
                            Statistic = StatisticMapper.Map(statistic, new DateOnlyColumnFactory()),
                            Subtitle = $"Category: {filter.Category?.Name}",
                            FirstColumnName = "Month",
                        }, 
                        cancellationToken);
                }
            }
        }
    }
}