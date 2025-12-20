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

            Task<List<IMoneyTransfer>>[] tasks;
            using (cancellationTokenSource)
            {
                tasks =
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

            Money monthOutcomes = FinanceCalculator.Sum(outcomes, financeFilter.Currency ?? Currency.RUR,
                dateFrom);
            Money monthIncomes = FinanceCalculator.Sum(incomes, financeFilter.Currency ?? Currency.RUR,
                dateFrom);

            var salaryCategory = Category.FromString("Зарплата");
            var previousSalaryDay = incomes.Where(c => c.Category == salaryCategory).Max(c => c.Date);
            var salaryDay = salaryDayService.GetSalaryDay(previousSalaryDay);

            bool includeToday = dateTimeService.Now().Hour <= 18;

            var moneyLeft = financeStatistics.CalculateMoneyPerDay(monthIncomes, outcomes, dateTimeService.Today(), salaryDay);

            string postTableInfo = 
                $"{moneyLeft} can be spent daily till the payday {salaryDay.ToString("d MMMM yyyy")} (today {(includeToday? "is" : "isn't")} included)";

            var table = BuildTable(monthIncomes, monthOutcomes, dateFrom,
                financeFilter.Currency!, postTableInfo);
            
            await mediator.Publish(new BalanceStatisticCalculatedEvent()
            {
                SessionId = session.Id,
                LastSentMessageId = session.LastSentMessageId,
                Table = table
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

    private static Table BuildTable(Money monthIncomes, Money monthOutcomes, DateOnly dateFrom, Currency currency,
        string postTableInfo)
    {
        var table = new Table()
        {
            Title = "Balance",
            Subtitle = $"From {dateFrom.ToString("MMMM yyyy")}",
            FirstColumnName = "Balance",
            Currencies = [currency],
            PostTableInfo = postTableInfo
        };
        table.AddRow(new Row()
        {
            FirstColumnValue = "Income",
            CurrencyValues = new Dictionary<Currency, Money>()
                { [currency] = monthIncomes }
        });
        table.AddRow(new Row()
        {
            FirstColumnValue = "Outcome",
            CurrencyValues = new Dictionary<Currency, Money>() { [currency] = monthOutcomes }
        });
        table.AddRow(new Row());
        table.AddRow(new Row()
        {
            FirstColumnValue = "Total",
            CurrencyValues = new Dictionary<Currency, Money>()
                { [currency] = monthIncomes - monthOutcomes }
        });
        return table;
    }
}