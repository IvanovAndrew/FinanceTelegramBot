using Domain;
using Infrastructure;
using Infrastructure.Fns;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    public class StateFactory : IStateFactory
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IEnumerable<Category> _categories;
        private readonly IFnsService _fnsService;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger<StateFactory> _logger;
    
        public StateFactory(IDateTimeService dateTimeService, IEnumerable<Category> categories, IFnsService fnsService, 
            IExpenseRepository expenseRepository, ILogger<StateFactory> logger)
        {
            _dateTimeService = dateTimeService;
            _categories = categories;
            _fnsService = fnsService;
            _expenseRepository = expenseRepository;
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
            return new EnterPriceState(expenseBuilder, _logger);
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

        public IExpenseInfoState GetExpensesState<T>(IExpenseInfoState previousState, ExpenseFilter expenseFilter, ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions)
        {
            return new CollectExpensesByCategoryState<T>(previousState, expenseFilter, expensesAggregator, firstColumnName, tableOptions, _expenseRepository, _logger);
        }

        public IExpenseInfoState CreateEnterTheCategoryForManyExpenses(List<IExpense> expenses)
        {
            return new SaveAllExpensesState(expenses, _expenseRepository, _logger);
        }

        public IExpenseInfoState CreateCollectMonthStatisticState()
        {
            return new CollectMonthStatisticState(_dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateCollectCategoryExpensesByMonthsState()
        {
            return new CollectCategoryExpensesState(_categories, _dateTimeService.Today(), _logger);
        }
        
        public IExpenseInfoState CreateCollectSubcategoryExpensesByMonthsState()
        {
            return new CollectSubcategoryExpensesByMonthsState(_categories, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateCollectSubcategoriesForAPeriodState()
        {
            return new CollectCategoryExpensesBySubcategoriesForAPeriodState(_categories, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateEnterRawQrState()
        {
            return new EnterRawQrState(_fnsService, _logger);
        }

        public IExpenseInfoState CreateCheckInfoState()
        {
            return new CheckInfoState();
        }
    }
}