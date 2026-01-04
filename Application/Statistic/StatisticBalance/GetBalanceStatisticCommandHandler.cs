using Domain;
using Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class GetBalanceStatisticCommandHandler(
    IUserSessionService userSessionService,
    IFinanceRepository financeRepository,
    FinanceStatisticsService financeStatistics,
    ISalaryDayService salaryDayService,
    IDateTimeService dateTimeService,
    IMediator mediator,
    ILogger<GetBalanceStatisticCommandHandler> logger) : IRequestHandler<GetBalanceStatisticCommand>
{
    public async Task Handle(GetBalanceStatisticCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"[{nameof(GetBalanceStatisticCommandHandler)}] started {request}");
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session == null)
        {
            logger.LogWarning(
                $"[{nameof(GetBalanceStatisticCommandHandler)}] session {request.SessionId} hasn't been found");
            return;
        }

        await mediator.Publish(
            new BalanceStatisticCollectingStarted()
                { SessionId = session.Id, LastSentMessageId = session.LastSentMessageId }, cancellationToken);

        var cancellationTokenSource = session.CancellationTokenSource = new CancellationTokenSource();

        try
        {
            List<IMoneyTransfer> outcomes;
            List<IMoneyTransfer> incomes;

            var dateFrom = session.StatisticsOptions.DateFrom.Value;

            var financeFilter = new FinanceFilter()
            {
                Currency = session.StatisticsOptions.Currency,
                DateFrom = dateFrom.FirstDayOfMonth()
            };

            using (cancellationTokenSource)
            {
                Task<List<IMoneyTransfer>>[] tasks =
                [
                    financeRepository.ReadOutcomes(financeFilter, cancellationTokenSource.Token),
                    financeRepository.ReadIncomes(financeFilter, cancellationTokenSource.Token)
                ];

                var results = await Task.WhenAll(tasks);
                outcomes = results[0];
                incomes = results[1];
            }

            logger.LogInformation($"{outcomes.Count} outcomes satisfy the requirements");
            logger.LogInformation($"{incomes.Count} incomes satisfy the requirements");

            if (!outcomes.Any() && !incomes.Any())
            {
                await mediator.Publish(new NeitherIncomesNotOutcomesFoundEvent
                {
                    SessionId = session.Id,
                    LastSentMessageId = session.LastSentMessageId
                }, cancellationToken);
                return;
            }
            
            var now = dateTimeService.Now();
            var today = dateTimeService.Today();

            var period = new BalancePeriod(incomes, outcomes, financeFilter.Currency);

            var monthlyBalances = period.ByMonths(
                YearMonth.From(dateFrom),
                YearMonth.From(today)
            );

            var moneyLeft = Money.Zero(financeFilter.Currency);
            
            var salaryCategory = Category.FromString("Зарплата");
            DateOnly? salaryDay = null;

            if (incomes.Any(c => c.Category == salaryCategory))
            {
                var previousSalaryDay = incomes.Where(c => c.Category == salaryCategory).Max(c => c.Date);
                salaryDay = salaryDayService.GetSalaryDay(previousSalaryDay);
                moneyLeft = financeStatistics.CalculateMoneyPerDay(period.TotalIncome, outcomes, today, salaryDay!.Value);
            }
            
            bool includeToday = now.Hour <= 18;

            await mediator.Publish(new BalanceStatisticCalculatedEvent()
            {
                SessionId = session.Id,
                LastSentMessageId = session.LastSentMessageId,
                
                DateFrom = dateFrom,
                MonthBalances = monthlyBalances,
                MoneyLeft = moneyLeft,
                Currency = session.StatisticsOptions.Currency,
                SalaryDay = salaryDay,
                IncludeToday = includeToday
            }, cancellationToken);

            session.LastSentMessageId = null;
            session.QuestionnaireService = null;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation is canceled by a user");
            await mediator.Publish(new LongOperationCanceledEvent() { SessionId = session.Id }, cancellationToken);
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }

        logger.LogInformation($"[{nameof(GetBalanceStatisticCommandHandler)}] finishes");
    }
}