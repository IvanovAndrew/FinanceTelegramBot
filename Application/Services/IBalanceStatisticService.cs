using Domain;
using Domain.Services;

namespace Application.Services;

public interface IBalanceStatisticService
{
    Task<BalanceStatisticResult> Calculate(
        YearMonth dateFrom,
        Currency currency,
        CancellationToken cancellationToken);
}

public sealed class BalanceStatisticResult(
    Currency? currency,
    MonthRange monthRange,
    Money saldo,
    Money unpaidRecurring,
    Money dailyBudget,
    IReadOnlyList<MonthlyBalance> monthBalances, FinancialPeriod periodLeft)
{
    public Currency? Currency { get; } = currency;
    public MonthRange MonthRange { get; } = monthRange;
    public Money Saldo { get; } = saldo;
    public Money UnpaidRecurring { get; } = unpaidRecurring;
    public Money DailyBudget { get; } = dailyBudget;
    public IReadOnlyList<MonthlyBalance> MonthBalances { get; } = monthBalances;
    public FinancialPeriod PeriodLeft { get; } = periodLeft;
}

public class BalanceStatisticService(
    IFinanceRepository financeRepository,
    FinanceStatisticsService financeStatistics,
    ISalaryScheduleProvider salaryScheduleProvider,
    ISalaryDayService salaryDayService,
    IDateTimeService dateTimeService,
    ISpendingDayPolicy spendingDayPolicy/*,
    ILogger<BalanceStatisticService> logger*/)
    : IBalanceStatisticService
{
    public async Task<BalanceStatisticResult> Calculate(
        YearMonth dateFrom,
        Currency currency,
        CancellationToken cancellationToken)
    {
        var now = dateTimeService.Now();

        var (outcomes, incomes) = await LoadIncomesAndOutcomes(dateFrom, currency, cancellationToken, now);

        if (!outcomes.Any() && !incomes.Any())
            throw new NoFinanceDataException();
        
        var today = dateTimeService.Today();
        var salarySchedule = salaryScheduleProvider.GetFrom(incomes);
        var salaryDay = salaryDayService.GetSalaryDay(salarySchedule.SalaryDay);
        var includeToday = spendingDayPolicy.CanInclude(now);
        
        var financialPeriod = new FinancialPeriod(today, salaryDay, includeToday);
        var monthRange = new MonthRange() { From = dateFrom, To = YearMonth.From(today) };
        
        var monthIncomes = FinanceCalculator.Sum(
            incomes,
            currency,
            monthRange);
        
        //logger.LogInformation($"Financial period {financialPeriod}");
        
        var (dailyBudget, unpaidRecurring) = financeStatistics.CalculateMoneyPerDay(
            monthIncomes,
            outcomes,
            dateFrom,
            financialPeriod);

        var balancePeriod = new BalancePeriod(incomes, outcomes, currency);

        var monthlyBalances = balancePeriod.ByMonths(monthRange);

        return new BalanceStatisticResult(
            currency,
            monthRange,
            balancePeriod.Saldo,
            unpaidRecurring,
            dailyBudget,
            monthlyBalances, financialPeriod);
    }

    private async Task<(List<Outcome> outcomes, List<Income> incomes)> LoadIncomesAndOutcomes(YearMonth dateFrom, Currency currency, CancellationToken cancellationToken,
        DateTime now)
    {
        var period = SpendingHistoryPeriod.FromCalculationStart(DateOnly.FromDateTime(now), dateFrom);

        var filter = new FinanceFilter { Currency = currency, DateFrom = period.From };

        var outcomesTask = financeRepository.ReadOutcomes(filter, cancellationToken);
        var incomesTask = financeRepository.ReadIncomes(filter with { Currency = null }, cancellationToken);

        await Task.WhenAll(outcomesTask, incomesTask);

        var outcomes = await outcomesTask;
        var incomes = await incomesTask;
        return (outcomes, incomes);
    }
}

public class NoFinanceDataException : Exception
{
}