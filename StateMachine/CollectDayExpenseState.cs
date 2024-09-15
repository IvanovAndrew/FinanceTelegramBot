using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CollectDayExpenseState : IExpenseInfoState
    {
        public bool UserAnswerIsRequired => true;
        private readonly DateOnly _today;
        
        private readonly string _dateFormat = "dd MMMM yyyy";
        private readonly StateChain _stateChain;
        private readonly ExpenseFilter _expenseFilter;
        private readonly ILogger _logger;

        public CollectDayExpenseState(DateOnly today, ILogger logger)
        {
            _today = today;
            _expenseFilter = new ExpenseFilter();

            _stateChain = new StateChain(this, 
                new DatePickerState(FilterUpdateStrategy<DateOnly>.FillDate(_expenseFilter), "Enter the day", today, _dateFormat,
                new[] { _today, _today.AddDays(-1) }, "Another day"), 
                new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_expenseFilter)));
            
            _logger = logger;
        }
    
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            return await _stateChain.Request(botClient, chatId, cancellationToken);
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            _stateChain.Handle(message, cancellationToken);
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) =>
            stateFactory.CreateChooseStatisticState();

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            var nextState = _stateChain.ToNextState();

            if (nextState.IsOutOfChain)
            {
                var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc: false);

                return stateFactory.GetExpensesState(this, _expenseFilter,
                    expenseAggregator,
                    s => s,
                    new TableOptions()
                    {
                        Title = _expenseFilter.DateTo.Value.ToString(_dateFormat),
                        FirstColumnName = "Category"
                    });
            }

            return this;
        }
    }
}