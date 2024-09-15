using Domain;
using Infrastructure;

namespace StateMachine;

internal abstract class UpdateStrategy<T>
{
    internal abstract void Update(T day);
}

internal class UpdateIncomeDateStrategy : UpdateStrategy<DateOnly>
{
    private readonly Income _income;

    internal UpdateIncomeDateStrategy(Income income)
    {
        _income = income;
    }
    
    internal override void Update(DateOnly day)
    {
        _income.Date = day;
    }
}

internal class UpdateOutcomeDateStrategy : UpdateStrategy<DateOnly>
{
    private readonly ExpenseBuilder _expense;

    internal UpdateOutcomeDateStrategy(ExpenseBuilder expense)
    {
        _expense = expense;
    }
    
    internal override void Update(DateOnly day)
    {
        _expense.Date = day;
    }
}

internal abstract class FilterUpdateStrategy<T> : UpdateStrategy<T>
{
    protected readonly FinanceFilter FinanceFilter;

    protected FilterUpdateStrategy(FinanceFilter financeFilter)
    {
        FinanceFilter = financeFilter;
    }

    internal static FilterUpdateStrategy<DateOnly> FillMonthRange(FinanceFilter financeFilter) =>
        new ExpensesFromAMonthRangeStrategy(financeFilter);

    internal static FilterUpdateStrategy<DateOnly> FillMonthFrom(FinanceFilter financeFilter) =>
        new ExpensesFromAMonthStrategy(financeFilter);

    internal static FilterUpdateStrategy<DateOnly> FillDate(FinanceFilter financeFilter) =>
        new ExpensesOneDayStrategy(financeFilter);

    public static FilterUpdateStrategy<Currency> FillCurrency(FinanceFilter financeFilter) =>
        new ExpensesByCurrencyStrategy(financeFilter);
    
    public static FilterUpdateStrategy<string> FillCategory(FinanceFilter financeFilter) =>
        new ExpensesByCategoryStrategy(financeFilter);
    
    public static FilterUpdateStrategy<string> FillSubcategory(FinanceFilter financeFilter) =>
        new ExpensesBySubcategoryStrategy(financeFilter);
}

internal class ExpensesOneDayStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesOneDayStrategy(FinanceFilter financeFilter) : base(financeFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        FinanceFilter.DateFrom = day;
        FinanceFilter.DateTo = day;
    }
}

internal class ExpensesFromAMonthRangeStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesFromAMonthRangeStrategy(FinanceFilter financeFilter) : base(financeFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        FinanceFilter.DateFrom = day.FirstDayOfMonth();
        FinanceFilter.DateTo = day.LastDayOfMonth();
    }
}

internal class ExpensesFromAMonthStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesFromAMonthStrategy(FinanceFilter financeFilter) : base(financeFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        FinanceFilter.DateFrom = day.FirstDayOfMonth();
    }
}

internal class ExpensesByCurrencyStrategy : FilterUpdateStrategy<Currency>
{
    protected internal ExpensesByCurrencyStrategy(FinanceFilter financeFilter) : base(financeFilter)
    {
    }

    internal override void Update(Currency day)
    {
        FinanceFilter.Currency = day;
    }
}

internal class ExpensesByCategoryStrategy : FilterUpdateStrategy<string>
{
    public ExpensesByCategoryStrategy(FinanceFilter financeFilter) : base(financeFilter)
    {
    }

    internal override void Update(string category)
    {
        FinanceFilter.Category = category;
    }
}

internal class ExpensesBySubcategoryStrategy : FilterUpdateStrategy<string>
{
    public ExpensesBySubcategoryStrategy(FinanceFilter financeFilter) : base(financeFilter)
    {
    }

    internal override void Update(string subcategory)
    {
        FinanceFilter.Subcategory = subcategory;
    }
}