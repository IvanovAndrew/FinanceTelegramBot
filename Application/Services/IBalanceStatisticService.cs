using Domain;
using Domain.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public interface IBalanceStatisticService
{
    Task<BalanceStatisticResult> Calculate(
        DateOnly dateFrom,
        Currency currency,
        CancellationToken cancellationToken);
}

public sealed class BalanceStatisticResult(
    DateOnly dateFrom,
    Currency? currency,
    bool includeToday,
    Money moneyLeft,
    DateOnly salaryDay,
    IReadOnlyList<MonthlyBalance> monthBalances)
{
    public DateOnly DateFrom { get; } = dateFrom;
    public Currency? Currency { get; } = currency;
    public bool IncludeToday { get; } = includeToday;
    public Money MoneyLeft { get; } = moneyLeft;
    public DateOnly SalaryDay { get; } = salaryDay;
    public IReadOnlyList<MonthlyBalance> MonthBalances { get; } = monthBalances;
}

public class BalanceStatisticService(
    IFinanceRepository financeRepository,
    FinanceStatisticsService financeStatistics,
    ISalaryScheduleProvider salaryScheduleProvider,
    ISalaryDayService salaryDayService,
    IDateTimeService dateTimeService,
    ISpendingDayPolicy spendingDayPolicy,
    ILogger<BalanceStatisticService> logger)
    : IBalanceStatisticService
{
    public async Task<BalanceStatisticResult> Calculate(
        DateOnly dateFrom,
        Currency currency,
        CancellationToken cancellationToken)
    {
        var now = dateTimeService.Now();

        var period = SpendingHistoryPeriod.FromCalculationStart(DateOnly.FromDateTime(now), dateFrom);
        logger.LogInformation($"{period.From} - {period.To}");

        var filter = new FinanceFilter { Currency = currency, DateFrom = period.From };

        var outcomesTask = financeRepository.ReadOutcomes(filter, cancellationToken);
        var incomesTask = financeRepository.ReadIncomes(filter with { Currency = null }, cancellationToken);

        await Task.WhenAll(outcomesTask, incomesTask);

        var outcomes = await outcomesTask;
        var incomes = (await incomesTask).Cast<Income>().ToList();

        logger.LogInformation($"Incomes {incomes.Count}");

        foreach (var income in incomes)
        {
            logger.LogInformation($"Income: {income.Category} {income.Description} {income.IsSalary()}");
        }

        if (!outcomes.Any() && !incomes.Any())
            throw new NoFinanceDataException();

        var monthIncomes = FinanceCalculator.Sum(
            incomes,
            currency,
            period.From);

        var salarySchedule = salaryScheduleProvider.GetFrom(incomes);
        var salaryDay = salaryDayService.GetSalaryDay(salarySchedule.SalaryDay);

        var includeToday = spendingDayPolicy.CanInclude(now);

        var today = dateTimeService.Today();
        var moneyLeft = financeStatistics.CalculateMoneyPerDay(
            monthIncomes,
            outcomes,
            today,
            salaryDay);

        var balancePeriod = new BalancePeriod(incomes, outcomes, currency);

        var monthlyBalances = balancePeriod.ByMonths(YearMonth.From(dateFrom), YearMonth.From(today));

        return new BalanceStatisticResult(
            dateFrom,
            currency,
            includeToday,
            moneyLeft,
            salaryDay,
            monthlyBalances);
    }
}

public class NoFinanceDataException : Exception
{
}