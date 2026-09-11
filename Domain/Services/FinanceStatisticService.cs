using Domain;
using Domain.Services;

public record FinancialPeriod
{
    public DateOnly Start { get; }
    public DateOnly End { get; }
    public bool IncludeStart { get; }

    public FinancialPeriod(DateOnly start, DateOnly end, bool includeStart = true)
    {
        if (end < start) 
            throw new ArgumentException("End date cannot be before start date.");
            
        Start = start;
        End = end;
        IncludeStart = includeStart;
    }

    public int DaysRemaining => 
        Math.Max(1, (End.DayNumber - Start.DayNumber) + (IncludeStart ? 1 : 0));
    
    public bool Includes(DateOnly date) => Start <= date && date <= End;
}

public record DailyBudget(Money dailyMoney, Money futureExpensesSum, IReadOnlyList<MissingRecurringExpense> futureExpenses);

public class FinanceStatisticsService(IRecurringExpensesService recurringExpensesService/*, ILogger<FinanceStatisticsService> logger*/)
{
    public DailyBudget CalculateMoneyPerDay(
        Money balance, IEnumerable<Outcome> outcomes, YearMonth monthFrom, FinancialPeriod period, DateOnly today,
        IReadOnlyCollection<RecurringExpenseDefinition> recurringDefinitions,
        IReadOnlyCollection<Outcome> recurringHistory)
    {
        var zero = Money.Zero(balance.Currency);

        var spentSoFar = outcomes
            .Where(o => monthFrom.ToDateOnly() <= o.Date)
            .Select(o => o.Amount)
            .Aggregate(zero, (a, b) => a + b);

        var missingRecurring = recurringExpensesService.GetMissingRecurringExpenses(recurringDefinitions, recurringHistory, today);
        var unpaidRecurring = missingRecurring.Select(m => m.ResolvedAmount ?? zero).Aggregate(zero, (a, b) => a + b);

        var moneyLeft = balance - spentSoFar - unpaidRecurring;
        var dailyMoney = moneyLeft.Amount > 0 ? moneyLeft / period.DaysRemaining : zero;

        return new DailyBudget(dailyMoney, unpaidRecurring, missingRecurring);
    }
}