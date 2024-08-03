using Domain;
using Infrastructure;

namespace StateMachine;

internal abstract class FilterUpdateStrategy<T>
{
    protected readonly ExpenseFilter ExpenseFilter;

    protected FilterUpdateStrategy(ExpenseFilter expenseFilter)
    {
        ExpenseFilter = expenseFilter;
    }

    internal static FilterUpdateStrategy<DateOnly> FillMonthRange(ExpenseFilter expenseFilter) =>
        new ExpensesFromADayStrategy(expenseFilter);

    internal static FilterUpdateStrategy<DateOnly> FillMonthFrom(ExpenseFilter expenseFilter) =>
        new ExpensesFromAMonthStrategy(expenseFilter);

    internal static FilterUpdateStrategy<DateOnly> FillDate(ExpenseFilter expenseFilter) =>
        new ExpensesOneDayStrategy(expenseFilter);

    public static FilterUpdateStrategy<Currency> FillCurrency(ExpenseFilter expenseFilter) =>
        new ExpensesByCurrencyStrategy(expenseFilter);

    internal abstract void Update(T day);
}

internal class ExpensesOneDayStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesOneDayStrategy(ExpenseFilter expenseFilter) : base(expenseFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        ExpenseFilter.DateFrom = day;
        ExpenseFilter.DateTo = day;
    }
}

internal class ExpensesFromADayStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesFromADayStrategy(ExpenseFilter expenseFilter) : base(expenseFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        ExpenseFilter.DateFrom = day.FirstDayOfMonth();
    }
}

internal class ExpensesFromAMonthStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesFromAMonthStrategy(ExpenseFilter expenseFilter) : base(expenseFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        ExpenseFilter.DateFrom = day.FirstDayOfMonth();
        ExpenseFilter.DateTo = day.LastDayOfMonth();
    }
}

internal class ExpensesByCurrencyStrategy : FilterUpdateStrategy<Currency>
{
    protected internal ExpensesByCurrencyStrategy(ExpenseFilter expenseFilter) : base(expenseFilter)
    {
    }

    internal override void Update(Currency day)
    {
        ExpenseFilter.Currency = day;
    }
}

