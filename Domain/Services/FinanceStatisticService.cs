using Domain;
using Domain.Services;

public class FinanceStatisticsService(IRecurringExpensesService recurringExpensesService)
{
    public Money CalculateMoneyPerDay(
        Money balance,
        IEnumerable<IMoneyTransfer> outcomes,
        DateOnly today,
        DateOnly nextSalaryDay
    )
    {
        var zero = Money.Zero(balance.Currency);
        
        var expenses = outcomes
            .Select(o => o.Amount)
            .Aggregate(zero, (a, b) => a + b);

        var unpaidRecurring = recurringExpensesService.GetMissingRecurringExpenses(outcomes, today)
            .Aggregate(zero, (a, b) => a + b);

        int remainingDays = nextSalaryDay.DayNumber - today.DayNumber;
        if (remainingDays <= 0) remainingDays = 1;

        var moneyLeft = balance - expenses - unpaidRecurring;

        return moneyLeft / remainingDays;
    }
}