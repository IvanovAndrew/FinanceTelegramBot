﻿using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    public class StateFactory : IStateFactory
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IMoneyParser _moneyParser;
        private readonly IEnumerable<Category> _categories;
        private readonly IFnsService _fnsService;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger<StateFactory> _logger;
    
        public StateFactory(IDateTimeService dateTimeService, IMoneyParser moneyParser, IEnumerable<Category> categories, IFnsService fnsService, 
            IExpenseRepository expenseRepository, ILogger<StateFactory> logger)
        {
            _dateTimeService = dateTimeService;
            _categories = categories;
            _fnsService = fnsService;
            _expenseRepository = expenseRepository;
            _moneyParser = moneyParser;
            _logger = logger;
        }

        public IExpenseInfoState CreateGreetingState()
        {
            return new GreetingState( _logger);
        }
        
        public IExpenseInfoState WayOfEnteringExpenseState()
        {
            return new EnterTheWayState(_logger);
        }

        public IExpenseInfoState CreateRequestPasteJsonState()
        {
            return new RequestJsonState(_logger);
        }

        public IExpenseInfoState CreateEnterTheDateState(IExpenseInfoState previousState, bool askCustomDate = false)
        {
            return new EnterTheDateState(_dateTimeService, _logger, previousState, askCustomDate);
        }

        public IExpenseInfoState CreateChooseStatisticState()
        {
            return new CreateStatisticTypeState(_logger);
        }

        public IExpenseInfoState CreateCategoryForStatisticState()
        {
            return new EnterCategoryForStatisticState(_categories, _logger);
        }

        public IExpenseInfoState CreateCollectDayExpenseState()
        {
            return new CollectDayExpenseState(_dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateEnterTheCategoryState(ExpenseBuilder expenseBuilder)
        {
            return new EnterTheCategoryState(expenseBuilder, _categories, _logger);
        }

        public IExpenseInfoState CreateEnterTheSubcategoryState(ExpenseBuilder expenseBuilder, SubCategory[] subCategories)
        {
            return new EnterSubcategoryState(expenseBuilder, subCategories, _logger);
        }

        public IExpenseInfoState CreateEnterDescriptionState(ExpenseBuilder expenseBuilder)
        {
            return new EnterDescriptionState(expenseBuilder, _logger);
        }

        public IExpenseInfoState CreateEnterThePriceState(ExpenseBuilder expenseBuilder)
        {
            return new EnterPriceState(expenseBuilder, _moneyParser, _logger);
        }

        public IExpenseInfoState CreateConfirmState(IExpense expense)
        {
            return new ConfirmExpenseState(expense, _categories, _logger);
        }

        public IExpenseInfoState CreateSaveState(IExpense expense)
        {
            return new SaveExpenseState(expense, _expenseRepository, _logger);
        }
        
        public IExpenseInfoState CreateSaveExpensesFromJsonState(List<IExpense> expenses)
        {
            return new SaveExpensesFromJsonState(expenses, _expenseRepository, _logger);
        }
        
        public IExpenseInfoState CreateHandleJsonFileState(ITelegramFileInfo fileInfo)
        {
            return new HandleJsonState(fileInfo, _logger);
        }

        public IExpenseInfoState CreateErrorWithRetryState(string warning, IExpenseInfoState state)
        {
            return new ErrorWithRetry(warning, state);
        }

        public IExpenseInfoState CreateCancelState()
        {
            return new CancelledState( _logger);
        }

        public IExpenseInfoState GetExpensesState<T>(IExpenseInfoState previousState, ISpecification<IExpense> specification, ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions)
        {
            return new CollectExpensesByCategoryState<T>(previousState, specification, expensesAggregator, firstColumnName, tableOptions, _expenseRepository, _logger);
        }

        public IExpenseInfoState CreateEnterTypeOfCategoryStatistic(Category category, IExpenseInfoState previousState)
        {
            return new EnterTypeOfCategoryStatisticState(category, _dateTimeService.Today(), previousState, _logger);
        }

        public IExpenseInfoState CreateEnterTheCategoryForManyExpenses(List<IExpense> expenses)
        {
            return new SaveAllExpensesState(expenses, _expenseRepository, _logger);
        }

        public IExpenseInfoState CreateCollectMonthStatisticState()
        {
            return new CollectMonthStatisticState(_dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateCollectCategoryExpensesByMonthsState(Category category)
        {
            return new CollectCategoryExpensesState(_dateTimeService.Today(), category, _logger);
        }
        
        public IExpenseInfoState CreateCollectSubcategoryExpensesByMonthsState(Category category, SubCategory subCategory)
        {
            return new CollectSubCategoryExpensesByMonthsState(_dateTimeService.Today(), category, subCategory, _logger);
        }

        public IExpenseInfoState CreateCollectCategoryExpensesBySubcategoriesForAPeriodState(Category category)
        {
            return new CollectCategoryExpensesBySubcategoriesForAPeriodState(category, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState EnterSubcategoryStatisticState(IExpenseInfoState previousState, Category category)
        {
            return new EnterSubcategoryStatisticState(category, previousState, _logger);
        }

        public IExpenseInfoState CreateEnterRawQrState()
        {
            return new EnterRawQrState(_fnsService, _logger);
        }
    }
}