using Domain;
using Domain.Services;

namespace Application.Core.Services;

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
    Money futureExpensesSum,
    IReadOnlyCollection<MissingRecurringExpense> futureExpenses,
    Money dailyBudget,
    IReadOnlyList<MonthlyBalance> monthBalances, FinancialPeriod periodLeft)
{
    public Currency? Currency { get; } = currency;
    public MonthRange MonthRange { get; } = monthRange;
    public Money Saldo { get; } = saldo;
    public Money FutureExpensesSum { get; } = futureExpensesSum;
    public IReadOnlyCollection<MissingRecurringExpense> FutureExpenses { get; } = futureExpenses;
    public Money DailyBudget { get; } = dailyBudget;
    public IReadOnlyList<MonthlyBalance> MonthBalances { get; } = monthBalances;
    public FinancialPeriod PeriodLeft { get; } = periodLeft;
}

public class BalanceStatisticService(
    IFinanceRepository financeRepository,
    FinanceStatisticsService financeStatistics,
    IRecurringExpenseDefinitionsRepository recurringExpenseDefinitionsRepository,
    ISalaryScheduleProvider salaryScheduleProvider,
    ISalaryDayService salaryDayService,
    IDateTimeService dateTimeService,
    ISpendingDayPolicy spendingDayPolicy/*,
    ILogger<BalanceStatisticService> logger*/)
    : IBalanceStatisticService
{
    public async Task<BalanceStatisticResult> Calculate(YearMonth dateFrom, Currency currency, CancellationToken cancellationToken)
    {
        var now = dateTimeService.Now();
        var today = dateTimeService.Today();
        var period = SpendingHistoryPeriod.FromCalculationStart(today, dateFrom); // считаем один раз, переиспользуем ниже

        var recurringDefinitions = await recurringExpenseDefinitionsRepository.GetActiveDefinitions(currency, cancellationToken);

        var (outcomes, incomes, currencyExchanges) = await LoadIncomesAndOutcomes(period, currency, cancellationToken);
        var recurringHistory = await LoadExtraRecurringHistoryIfNeeded(outcomes, recurringDefinitions, period, today, currency, cancellationToken);

        if (!outcomes.Any() && !incomes.Any())
            throw new NoFinanceDataException();

        var allIncomes = incomes.Union(currencyExchanges.Where(x => x.TargetAmount.Currency == currency).Select(x => new Income(){Date = x.Date, Amount = x.TargetAmount}));
        var allOutcomes = outcomes.Union(currencyExchanges.Where(x => x.SourceAmount.Currency == currency).Select(x => new Outcome(){Date = x.Date, Amount = x.SourceAmount}));

        var monthRange = new MonthRange { From = dateFrom, To = YearMonth.From(today) };
        var financialPeriod = BuildFinancialPeriod(allIncomes, today, now);

        var monthlyBalances = new BalancePeriod(allIncomes, allOutcomes, currency).ByMonths(monthRange);
        var saldo = SumSaldo(monthlyBalances, currency);

        var monthIncome = FinanceCalculator.Sum(allIncomes, currency, monthRange);
        var (dailyBudget,  futureExpensesSum, futureExpenses) = financeStatistics.CalculateMoneyPerDay(
            monthIncome, allOutcomes, dateFrom, financialPeriod, today, recurringDefinitions, recurringHistory);
        
        return new BalanceStatisticResult(currency, monthRange, saldo, futureExpensesSum, futureExpenses, dailyBudget, monthlyBalances, financialPeriod);
    }

    private async Task<IReadOnlyList<Outcome>> LoadExtraRecurringHistoryIfNeeded(
        IReadOnlyList<Outcome> alreadyLoaded, IReadOnlyCollection<RecurringExpenseDefinition> recurringDefinitions,
        SpendingHistoryPeriod period, DateOnly today, Currency currency, CancellationToken ct)
    {
        var earliestRequired = RecurringExpenseHistoryRequirement.EarliestRequiredDate(recurringDefinitions, today);
        if (earliestRequired >= period.From)
            return alreadyLoaded;

        var extra = await financeRepository.ReadOutcomes(
            new FinanceFilter { Currency = currency, DateFrom = earliestRequired, DateTo = period.From.AddDays(-1) }, ct);

        return extra.Concat(alreadyLoaded).ToList();
    }
    
    private static Money SumSaldo(IReadOnlyList<MonthlyBalance> monthlyBalances, Currency currency) =>
        monthlyBalances
            .Select(mb => mb.Balance)
            .Aggregate(new Balance(Money.Zero(currency), Money.Zero(currency)), (acc, b) => acc + b)
            .Saldo;
    
    private FinancialPeriod BuildFinancialPeriod(IEnumerable<Income> incomes, DateOnly today, DateTime now)
    {
        var salarySchedule = salaryScheduleProvider.GetFrom(incomes);
        var salaryDay = salaryDayService.GetSalaryDay(salarySchedule.SalaryDay);
        var includeToday = spendingDayPolicy.CanInclude(now);

        return new FinancialPeriod(today, salaryDay, includeToday);
    }

    private async Task<(IReadOnlyList<Outcome> outcomes, IReadOnlyList<Income> incomes, IReadOnlyList<CurrencyExchange> currencyExchanges)> LoadIncomesAndOutcomes(
        SpendingHistoryPeriod period, Currency currency, CancellationToken cancellationToken)
    {
        var filter = new FinanceFilter { Currency = currency, DateFrom = period.From };

        var outcomesTask = financeRepository.ReadOutcomes(filter, cancellationToken);
        var incomesTask = financeRepository.ReadIncomes(filter with { Currency = null }, cancellationToken);
        var currencyExchangeTask = financeRepository.ReadCurrencyExchanges(currency, period.From, period.To, cancellationToken);

        await Task.WhenAll(outcomesTask, incomesTask, currencyExchangeTask);

        return (await outcomesTask, await incomesTask, await currencyExchangeTask);
    }
}