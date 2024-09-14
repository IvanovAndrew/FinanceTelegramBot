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

internal abstract class FilterUpdateStrategy<T> : UpdateStrategy<T>
{
    protected readonly FinanseFilter FinanseFilter;

    protected FilterUpdateStrategy(FinanseFilter finanseFilter)
    {
        FinanseFilter = finanseFilter;
    }

    internal static FilterUpdateStrategy<DateOnly> FillMonthRange(FinanseFilter finanseFilter) =>
        new ExpensesFromAMonthRangeStrategy(finanseFilter);

    internal static FilterUpdateStrategy<DateOnly> FillMonthFrom(FinanseFilter finanseFilter) =>
        new ExpensesFromAMonthStrategy(finanseFilter);

    internal static FilterUpdateStrategy<DateOnly> FillDate(FinanseFilter finanseFilter) =>
        new ExpensesOneDayStrategy(finanseFilter);

    public static FilterUpdateStrategy<Currency> FillCurrency(FinanseFilter finanseFilter) =>
        new ExpensesByCurrencyStrategy(finanseFilter);
    
    public static FilterUpdateStrategy<string> FillCategory(FinanseFilter finanseFilter) =>
        new ExpensesByCategoryStrategy(finanseFilter);
    
    public static FilterUpdateStrategy<string> FillSubcategory(FinanseFilter finanseFilter) =>
        new ExpensesBySubcategoryStrategy(finanseFilter);
}

internal class ExpensesOneDayStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesOneDayStrategy(FinanseFilter finanseFilter) : base(finanseFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        FinanseFilter.DateFrom = day;
        FinanseFilter.DateTo = day;
    }
}

internal class ExpensesFromAMonthRangeStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesFromAMonthRangeStrategy(FinanseFilter finanseFilter) : base(finanseFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        FinanseFilter.DateFrom = day.FirstDayOfMonth();
        FinanseFilter.DateTo = day.LastDayOfMonth();
    }
}

internal class ExpensesFromAMonthStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesFromAMonthStrategy(FinanseFilter finanseFilter) : base(finanseFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        FinanseFilter.DateFrom = day.FirstDayOfMonth();
    }
}

internal class ExpensesByCurrencyStrategy : FilterUpdateStrategy<Currency>
{
    protected internal ExpensesByCurrencyStrategy(FinanseFilter finanseFilter) : base(finanseFilter)
    {
    }

    internal override void Update(Currency day)
    {
        FinanseFilter.Currency = day;
    }
}

internal class ExpensesByCategoryStrategy : FilterUpdateStrategy<string>
{
    public ExpensesByCategoryStrategy(FinanseFilter finanseFilter) : base(finanseFilter)
    {
    }

    internal override void Update(string category)
    {
        FinanseFilter.Category = category;
    }
}

internal class ExpensesBySubcategoryStrategy : FilterUpdateStrategy<string>
{
    public ExpensesBySubcategoryStrategy(FinanseFilter finanseFilter) : base(finanseFilter)
    {
    }

    internal override void Update(string subcategory)
    {
        FinanseFilter.Subcategory = subcategory;
    }
}