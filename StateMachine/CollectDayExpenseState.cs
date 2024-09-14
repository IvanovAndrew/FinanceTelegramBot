using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CollectDayExpenseState : IExpenseInfoState
    {
        public bool UserAnswerIsRequired => true;
        private readonly DateOnly _today;
        
        private readonly string _dateFormat = "dd MMMM yyyy";
        private readonly StateChain _stateChain;
        private readonly FinanseFilter _finanseFilter;
        private readonly ILogger _logger;

        public CollectDayExpenseState(DateOnly today, ILogger logger)
        {
            _today = today;
            _finanseFilter = new FinanseFilter();

            _stateChain = new StateChain(
                new DatePickerState(FilterUpdateStrategy<DateOnly>.FillDate(_finanseFilter), "Enter the day", today, _dateFormat,
                new[] { _today, _today.AddDays(-1) }, "Another day"), 
                new CurrencyPicker(FilterUpdateStrategy<Currency>.FillCurrency(_finanseFilter)));
            
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

                return stateFactory.GetExpensesState(this, _finanseFilter,
                    expenseAggregator,
                    s => s,
                    new TableOptions()
                    {
                        Title = _finanseFilter.DateTo.Value.ToString(_dateFormat),
                        FirstColumnName = "Category"
                    });
            }

            return this;
        }
    }
}