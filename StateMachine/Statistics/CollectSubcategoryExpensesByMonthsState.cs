﻿using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine.Statistics;

internal class CollectSubcategoryExpensesByMonthsState : StateWithChainsBase
{
    private readonly DateOnly _today;
    private readonly ILogger _logger;

    private readonly FinanceFilter _financeFilter;
    private string DateFormat = "MMMM yyyy";

    public CollectSubcategoryExpensesByMonthsState(IEnumerable<Category> categories, DateOnly today, ILogger logger)
    {
        _today = today;
        _financeFilter = new FinanceFilter() {Income = false};
        
        StateChain = new StateChain(
            new CategorySubcategoryPicker(c => _financeFilter.Category = c.Name, sc => _financeFilter.Subcategory = sc.Name, categories, logger),
            new DatePickerState(FilterUpdateStrategy<DateOnly>.FillMonthFrom(_financeFilter), "Enter the start period", _today, DateFormat,
            new[] { _today.AddYears(-1), _today.AddMonths(-6), _today.AddMonths(-1) }.ToDictionary(d => d, d => d.ToString(DateFormat)), "Another period", logger), 
            new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_financeFilter)));
        
        _logger = logger;
    }

    protected override IExpenseInfoState PreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateChooseStatisticState();
    }

    protected override IExpenseInfoState NextState(IStateFactory stateFactory)
    {
        var expenseAggregator = new ExpensesAggregator<DateOnly>(
            e => e.Date.LastDayOfMonth(), sortByMoney:false, sortAsc: true);

        return stateFactory.GetExpensesState(this, _financeFilter,
            expenseAggregator,
            s => s.ToString(DateFormat),
            new TableOptions()
            {
                Title = $"Category: {_financeFilter.Category}. {Environment.NewLine}" +
                        $"Subcategory: {_financeFilter.Subcategory}. {Environment.NewLine}" +
                        $"Expenses from {_financeFilter.DateFrom.Value.ToString(DateFormat)}",
                FirstColumnName = "Month"
            });
    }
}