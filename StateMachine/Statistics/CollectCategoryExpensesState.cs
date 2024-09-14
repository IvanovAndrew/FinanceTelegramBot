﻿using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class CollectCategoryExpensesState : StateWithChainsBase
{
    private readonly ILogger _logger;
    private readonly FinanceFilter _financeFilter;
    private const string DateFormat = "MMMM yyyy";

    public CollectCategoryExpensesState(IEnumerable<Category> categories, DateOnly today, ILogger logger)
    {
        _logger = logger;
        _financeFilter = new FinanceFilter();

        StateChain = new StateChain(
            new CategoryPicker(c => _financeFilter.Category = c.Name, categories, _logger), 
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_financeFilter), "Enter the start period",
                today, DateFormat,
                new[] { today.AddYears(-1), today.AddMonths(-6), today.AddMonths(-1) }, "Another"),
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_financeFilter)));
    }

    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory) =>
        stateFactory.CreateChooseStatisticState();

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        var expenseAggregator = new ExpensesAggregator<DateOnly>(
            e => e.Date.LastDayOfMonth(), false, sortAsc: true);

        return stateFactory.GetExpensesState(this, _financeFilter, expenseAggregator,
            s => s.ToString(DateFormat),
            new TableOptions()
            {
                Title = $"Category: {_financeFilter.Category}",
                FirstColumnName = "Month"
            });
    }
}