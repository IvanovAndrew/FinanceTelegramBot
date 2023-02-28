﻿using System.Globalization;
using Domain;
using GoogleSheet;
using TelegramBot.StateMachine;

namespace TelegramBot;

internal class StateFactory
{
    private readonly IDateParser _dateParser;
    private readonly IMoneyParser _moneyParser;
    private readonly IEnumerable<Category> _categories;
    private readonly GoogleSheetWrapper _spreadsheetWrapper;
    private readonly ILogger _logger;
    
    public StateFactory(IDateParser dateParser, IEnumerable<Category> categories, IMoneyParser moneyParser,
        GoogleSheetWrapper spreadsheetWrapper, ILogger logger)
    {
        _dateParser = dateParser;
        _categories = categories;
        _spreadsheetWrapper = spreadsheetWrapper;
        _moneyParser = moneyParser;
        _logger = logger;
    }

    internal IExpenseInfoState CreateGreetingState()
    {
        return new GreetingState(this, _logger);
    }
    
    internal IExpenseInfoState CreateEnterTheDateState(IExpenseInfoState previousState, bool askCustomDate = false)
    {
        return new EnterTheDateState(this, previousState, _dateParser, _logger, askCustomDate);
    }
    
    internal IExpenseInfoState CreateEnterTheMonthState(IExpenseInfoState previousState)
    {
        return new EnterTheMonthState(this, previousState, DateOnly.FromDateTime(DateTime.Today), _logger);
    }

    internal IExpenseInfoState CreateEnterTheCategoryState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState)
    {
        return new EnterTheCategoryState(this, previousState, expenseBuilder, _categories, _logger);
    }
    
    internal IExpenseInfoState CreateEnterTheSubcategoryState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState, SubCategory[] subCategories)
    {
        return new EnterSubcategoryState(this, previousState, expenseBuilder, subCategories, _logger);
    }

    internal IExpenseInfoState CreateEnterDescriptionState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState)
    {
        return new EnterDescriptionState(this, previousState, expenseBuilder, _logger);
    }
    
    internal IExpenseInfoState CreateEnterThePriceState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState)
    {
        return new EnterPriceState(this, previousState, expenseBuilder, _moneyParser, _logger);
    }

    public IExpenseInfoState CreateConfirmState(IExpense expense, IExpenseInfoState previousState)
    {
        return new ConfirmExpenseState(this, previousState, expense, _logger);
    }

    public IExpenseInfoState CreateSaveState(IExpenseInfoState previousState, IExpense expense)
    {
        return new SaveExpenseState(this, previousState, expense, _spreadsheetWrapper, _logger);
    }

    public IExpenseInfoState CreateErrorWithRetryState(string warning, IExpenseInfoState previousState)
    {
        return new ErrorWithRetry(warning, previousState);
    }

    public IExpenseInfoState CreateCancelState()
    {
        return new CancelledState(this, _logger);
    }

    public IExpenseInfoState GetMonthExpensesState(IExpenseInfoState previousState, DateOnly selectedMonth)
    {
        return new MonthExpensesState(this, previousState, selectedMonth, _spreadsheetWrapper, _logger);
    }
}

public interface IDateParser
{
    bool TryParse(string s, out DateOnly date);
}

class DateParser : IDateParser
{
    private readonly CultureInfo _cultureInfo;
    public DateParser(CultureInfo cultureInfo)
    {
        _cultureInfo = cultureInfo;
    }
    
    public bool TryParse(string s, out DateOnly date)
    {
        if (string.IsNullOrEmpty(s))
            return false;
        else if (string.Equals(s, "today", StringComparison.InvariantCultureIgnoreCase))
        {
            date = DateOnly.FromDateTime(DateTime.Today);
            return true;
        }
        else if (string.Equals(s, "yesterday", StringComparison.InvariantCultureIgnoreCase))
        {
            date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            return true;
        }

        return DateOnly.TryParse(s, _cultureInfo, DateTimeStyles.None, out date);
    }
} 