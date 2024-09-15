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
        private readonly IEnumerable<IncomeCategory> _incomeCategories = new[]
        {
            new IncomeCategory(){Name = "Зарплата"},
            new IncomeCategory(){Name = "Премия"},
            new IncomeCategory(){Name = "Отпускные"},
            new IncomeCategory(){Name = "Кэшбек"},
            new IncomeCategory(){Name = "% на остаток"},
            new IncomeCategory(){Name = "Аренда квартиры"},
            new IncomeCategory(){Name = "Прочее"},
        };
        
        private readonly IFnsService _fnsService;
        private readonly IFinanseRepository _finanseRepository;
        private readonly ILogger<StateFactory> _logger;
    
        public StateFactory(IDateTimeService dateTimeService, IEnumerable<Category> categories, IFnsService fnsService, 
            IFinanseRepository finanseRepository, ILogger<StateFactory> logger)
        {
            _dateTimeService = dateTimeService;
            _categories = categories;
            _fnsService = fnsService;
            _finanseRepository = finanseRepository;
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

        public IExpenseInfoState CreateChooseStatisticState()
        {
            return new CreateStatisticTypeState(_logger);
        }

        public IExpenseInfoState CreateCollectDayExpenseState()
        {
            return new CollectDayExpenseState(_dateTimeService.Today(), _logger);
        }

        public IExpenseInfoState CreateConfirmState(IExpense expense)
        {
            return new ConfirmExpenseState(expense, _categories, _logger);
        }

        public IExpenseInfoState CreateConfirmState(IIncome income)
        {
            return new ConfirmIncomeState(income, _logger);
        }

        public IExpenseInfoState CreateSaveState(IExpense expense)
        {
            return new SaveExpenseState(expense, _finanseRepository, _logger);
        }
        
        public IExpenseInfoState CreateSaveState(IIncome expense)
        {
            return new SaveIncomeState(expense, _finanseRepository, _logger);
        }

        public IExpenseInfoState CreateSaveExpensesFromJsonState(List<IExpense> expenses)
        {
            return new SaveExpensesFromJsonState(expenses, _finanseRepository, _logger);
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

        public IExpenseInfoState GetExpensesState<T>(IExpenseInfoState previousState, FinanceFilter financeFilter, ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions)
        {
            return new CollectExpensesByCategoryState<T>(previousState, financeFilter, expensesAggregator, firstColumnName, tableOptions, _finanseRepository, _logger);
        }

        public IExpenseInfoState CreateEnterTheCategoryForManyExpenses(List<IExpense> expenses)
        {
            return new SaveAllExpensesState(expenses, _finanseRepository, _logger);
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

        public IExpenseInfoState CreateCheckByRequisitesState()
        {
            return new EnterCheckRequisitesState(_dateTimeService.Now(), _logger);
        }

        public IExpenseInfoState CreateRequestFnsDataState(string messageText)
        {
            return new RequestFnsDataState(_fnsService, messageText, _logger);
        }

        public IExpenseInfoState CreateEnterIncomeState()
        {
            return new EnterIncomeState(_dateTimeService.Today(), _incomeCategories, _logger);
        }

        public IExpenseInfoState CreateEnterOutcomeManuallyState()
        {
            return new EnterOutcomeManuallyState(_dateTimeService.Today(), _categories, _logger);
        }
    }
}