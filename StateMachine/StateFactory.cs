using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    public class StateFactory
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IMoneyParser _moneyParser;
        private readonly IEnumerable<Category> _categories;
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILogger<StateFactory> _logger;
    
        public StateFactory(IDateTimeService dateTimeService, IMoneyParser moneyParser, IEnumerable<Category> categories, 
            IExpenseRepository expenseRepository, ILogger<StateFactory> logger)
        {
            _dateTimeService = dateTimeService;
            _categories = categories;
            _expenseRepository = expenseRepository;
            _moneyParser = moneyParser;
            _logger = logger;
        }

        public IExpenseInfoState CreateGreetingState()
        {
            return new GreetingState(this, _logger);
        }
        
        internal IExpenseInfoState WayOfEnteringExpenseState(IExpenseInfoState previousState)
        {
            return new EnterTheWayState(this, previousState, _logger);
        }
        
        internal IExpenseInfoState CreateRequestPasteJsonState(IExpenseInfoState previousState)
        {
            return new RequestJsonState(this, previousState, _logger);
        }
    
        internal IExpenseInfoState CreateEnterTheDateState(IExpenseInfoState previousState, bool askCustomDate = false)
        {
            return new EnterTheDateState(this, previousState, _dateTimeService, _logger, askCustomDate);
        }
    
        internal IExpenseInfoState CreateChooseStatisticState(IExpenseInfoState previousState)
        {
            return new CreateStatisticTypeState(this, previousState, _logger);
        }

        internal IExpenseInfoState CreateCategoryForStatisticState(IExpenseInfoState previousState)
        {
            return new EnterCategoryForStatisticState(this, previousState, _categories, _logger);
        }
    
        internal IExpenseInfoState CreateCollectDayExpenseState(IExpenseInfoState previousState)
        {
            return new CollectDayExpenseState(this, previousState, _dateTimeService.Today(), _logger);
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
            return new SaveExpenseState(this, previousState, expense, _expenseRepository, _logger);
        }
        
        public IExpenseInfoState CreateSaveExpensesFromJsonState(IExpenseInfoState previousState, List<IExpense> expenses)
        {
            return new SaveExpensesFromJsonState(this, previousState, expenses, _expenseRepository, _logger);
        }
        
        public IExpenseInfoState CreateHandleJsonFileState(IExpenseInfoState previousState, ITelegramFileInfo fileInfo)
        {
            return new HandleJsonState(this, previousState, fileInfo, _logger);
        }

        public IExpenseInfoState CreateErrorWithRetryState(string warning, IExpenseInfoState previousState)
        {
            return new ErrorWithRetry(warning, previousState);
        }

        public IExpenseInfoState CreateCancelState()
        {
            return new CancelledState(this, _logger);
        }

        public IExpenseInfoState GetExpensesState<T>(IExpenseInfoState previousState, ISpecification<IExpense> specification, ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions)
        {
            return new CollectExpensesByCategoryState<T>(this, previousState, specification, expensesAggregator, firstColumnName, tableOptions, _expenseRepository, _logger);
        }

        public IExpenseInfoState GetEnterTypeOfCategoryStatistic(IExpenseInfoState previousState, Category category)
        {
            return new EnterTypeOfCategoryStatisticState(this, previousState, category.Name, _dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateEnterTheCategoryForManyExpenses(List<IExpense> expenses, IExpenseInfoState previousState)
        {
            return new SaveAllExpensesState(this, previousState, expenses, _expenseRepository, _logger);
        }

        internal IExpenseInfoState CreateCollectMonthStatisticState(IExpenseInfoState previousState)
        {
            return new CollectMonthStatisticState(this, previousState, _dateTimeService.Today(), _logger);
        }
    }
}