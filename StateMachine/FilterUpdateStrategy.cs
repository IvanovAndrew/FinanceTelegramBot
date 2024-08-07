﻿using Domain;
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
        new ExpensesFromAMonthRangeStrategy(expenseFilter);

    internal static FilterUpdateStrategy<DateOnly> FillMonthFrom(ExpenseFilter expenseFilter) =>
        new ExpensesFromAMonthStrategy(expenseFilter);

    internal static FilterUpdateStrategy<DateOnly> FillDate(ExpenseFilter expenseFilter) =>
        new ExpensesOneDayStrategy(expenseFilter);

    public static FilterUpdateStrategy<Currency> FillCurrency(ExpenseFilter expenseFilter) =>
        new ExpensesByCurrencyStrategy(expenseFilter);
    
    public static FilterUpdateStrategy<string> FillCategory(ExpenseFilter expenseFilter) =>
        new ExpensesByCategoryStrategy(expenseFilter);
    
    public static FilterUpdateStrategy<string> FillSubcategory(ExpenseFilter expenseFilter) =>
        new ExpensesBySubcategoryStrategy(expenseFilter);

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

internal class ExpensesFromAMonthRangeStrategy : FilterUpdateStrategy<DateOnly>
{
    public ExpensesFromAMonthRangeStrategy(ExpenseFilter expenseFilter) : base(expenseFilter)
    {
    }

    internal override void Update(DateOnly day)
    {
        ExpenseFilter.DateFrom = day.FirstDayOfMonth();
        ExpenseFilter.DateTo = day.LastDayOfMonth();
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

internal class ExpensesByCategoryStrategy : FilterUpdateStrategy<string>
{
    public ExpensesByCategoryStrategy(ExpenseFilter expenseFilter) : base(expenseFilter)
    {
    }

    internal override void Update(string category)
    {
        ExpenseFilter.Category = category;
    }
}

internal class ExpensesBySubcategoryStrategy : FilterUpdateStrategy<string>
{
    public ExpensesBySubcategoryStrategy(ExpenseFilter expenseFilter) : base(expenseFilter)
    {
    }

    internal override void Update(string subcategory)
    {
        ExpenseFilter.Subcategory = subcategory;
    }
}