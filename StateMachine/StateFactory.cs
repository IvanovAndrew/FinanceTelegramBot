using Domain;
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
        
        public IExpenseInfoState WayOfEnteringExpenseState(IExpenseInfoState previousState)
        {
            return new EnterTheWayState(previousState, _logger);
        }

        public IExpenseInfoState CreateRequestPasteJsonState(IExpenseInfoState previousState)
        {
            return new RequestJsonState(previousState, _logger);
        }

        public IExpenseInfoState CreateEnterTheDateState(IExpenseInfoState previousState, bool askCustomDate = false)
        {
            return new EnterTheDateState(previousState, _dateTimeService, _logger, askCustomDate);
        }

        public IExpenseInfoState CreateChooseStatisticState(IExpenseInfoState previousState)
        {
            return new CreateStatisticTypeState(previousState, _logger);
        }

        public IExpenseInfoState CreateCategoryForStatisticState(IExpenseInfoState previousState)
        {
            return new EnterCategoryForStatisticState(previousState, _categories, _logger);
        }

        public IExpenseInfoState CreateCollectDayExpenseState(IExpenseInfoState previousState)
        {
            return new CollectDayExpenseState(previousState, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateEnterTheCategoryState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState)
        {
            return new EnterTheCategoryState(previousState, expenseBuilder, _categories, _logger);
        }

        public IExpenseInfoState CreateEnterTheSubcategoryState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState, SubCategory[] subCategories)
        {
            return new EnterSubcategoryState(previousState, expenseBuilder, subCategories, _logger);
        }

        public IExpenseInfoState CreateEnterDescriptionState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState)
        {
            return new EnterDescriptionState(previousState, expenseBuilder, _logger);
        }

        public IExpenseInfoState CreateEnterThePriceState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState)
        {
            return new EnterPriceState(previousState, expenseBuilder, _moneyParser, _logger);
        }

        public IExpenseInfoState CreateConfirmState(IExpense expense, IExpenseInfoState previousState)
        {
            return new ConfirmExpenseState(previousState, expense, _logger);
        }

        public IExpenseInfoState CreateSaveState(IExpenseInfoState previousState, IExpense expense)
        {
            return new SaveExpenseState(previousState, expense, _expenseRepository, _logger);
        }
        
        public IExpenseInfoState CreateSaveExpensesFromJsonState(IExpenseInfoState previousState, List<IExpense> expenses)
        {
            return new SaveExpensesFromJsonState(previousState, expenses, _expenseRepository, _logger);
        }
        
        public IExpenseInfoState CreateHandleJsonFileState(IExpenseInfoState previousState, ITelegramFileInfo fileInfo)
        {
            return new HandleJsonState(previousState, fileInfo, _logger);
        }

        public IExpenseInfoState CreateErrorWithRetryState(string warning, IExpenseInfoState previousState)
        {
            return new ErrorWithRetry(warning, previousState);
        }

        public IExpenseInfoState CreateCancelState()
        {
            return new CancelledState( _logger);
        }

        public IExpenseInfoState GetExpensesState<T>(IExpenseInfoState previousState, ISpecification<IExpense> specification, ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions)
        {
            return new CollectExpensesByCategoryState<T>(previousState, specification, expensesAggregator, firstColumnName, tableOptions, _expenseRepository, _logger);
        }

        public IExpenseInfoState GetEnterTypeOfCategoryStatistic(IExpenseInfoState previousState, Category category)
        {
            return new EnterTypeOfCategoryStatisticState(previousState, category, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateEnterTheCategoryForManyExpenses(List<IExpense> expenses, IExpenseInfoState previousState)
        {
            return new SaveAllExpensesState(previousState, expenses, _expenseRepository, _logger);
        }

        public IExpenseInfoState CreateCollectMonthStatisticState(IExpenseInfoState previousState)
        {
            return new CollectMonthStatisticState(previousState, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateCollectCategoryExpensesByMonthsState(IExpenseInfoState previousState, Category category)
        {
            return new CollectCategoryExpensesState(previousState, _dateTimeService.Today(), category, _logger);
        }
        
        public IExpenseInfoState CreateCollectSubcategoryExpensesByMonthsState(IExpenseInfoState previousState,
            Category category, SubCategory subCategory)
        {
            return new CollectSubCategoryExpensesByMonthsState(previousState, _dateTimeService.Today(), category, subCategory, _logger);
        }

        public IExpenseInfoState CreateCollectCategoryExpensesBySubcategoriesForAPeriodState(IExpenseInfoState  previousState, Category category)
        {
            return new CollectCategoryExpensesBySubcategoriesForAPeriodState(previousState, category, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState EnterSubcategoryStatisticState(IExpenseInfoState previousState, Category category)
        {
            return new EnterSubcategoryStatisticState(previousState, category, _logger);
        }

        public IExpenseInfoState CreateEnterRawQrState(IExpenseInfoState previousState)
        {
            return new EnterRawQrState(previousState, _fnsService, _logger);
        }
    }
}