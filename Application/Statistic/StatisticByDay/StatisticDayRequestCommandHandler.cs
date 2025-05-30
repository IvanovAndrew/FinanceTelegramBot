﻿using Application.AddMoneyTransfer;
using Application.Commands.StatisticByDay;
using Application.Events;
using Domain;
using MediatR;

namespace Application.Statistic.StatisticByDay;

public class StatisticDayRequestCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<StatisticDayRequestCommand>
{
    public async Task Handle(StatisticDayRequestCommand dayRequest, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(dayRequest.SessionId);
        if (session != null)
        {
            var sessionStatisticsOptions = session.StatisticsOptions;
            
            var filter = new FinanceFilter()
            {
                DateFrom = sessionStatisticsOptions.DateFrom,
                DateTo = sessionStatisticsOptions.DateTo,
                Category = session.StatisticsOptions.Category,
                Currency = sessionStatisticsOptions.Currency,
            };
            
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, sortAsc: false);

            session.CancellationTokenSource = new CancellationTokenSource();

            try
            {
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
                            Statistic = StatisticMapper.Map(statistic, new StringColumnFactory()),
                            Subtitle = $"Expenses for {sessionStatisticsOptions.DateTo.Value.ToString("d MMMM yyyy")}",
                            FirstColumnName = "Category",
                            DateFrom = filter.DateFrom,
                            DateTo = filter.DateTo,
                        }, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                await mediator.Publish(new TaskCanceledEvent(){SessionId = session.Id}, cancellationToken);
            }
            finally
            {
                session.CancellationTokenSource = null;
            }
        }
    }
}