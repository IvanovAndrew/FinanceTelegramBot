using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterTheMonthState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => true;
    private readonly StateFactory _factory;
    private readonly DateOnly _today;
    private readonly ILogger _logger;
    private readonly string _dateFormat = "yyyy-MM-dd";
    
    public IExpenseInfoState PreviousState { get; private set; }

    public EnterTheMonthState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger)
    {
        _factory = factory;
        _today = today;
        _logger = logger;
        PreviousState = previousState;
    }
    
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        var info = "Enter the month";

        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            _today.AddMonths(-5), _today.AddMonths(-4), _today.AddMonths(-3),
            _today.AddMonths(-2), _today.AddMonths(-1), _today
        }.Select(date => new TelegramButton
            { Text = $"{date.ToString("MMMM yyyy")}", CallbackData = date.ToString(_dateFormat) }), chunkSize: 3);
        
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: info,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        if (DateOnly.TryParseExact(text, _dateFormat, out var selectedMonth))
        {
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, s => s, true, sortAsc:false);

            var specification =
                new ExpenseFromDateRangeSpecification(selectedMonth.FirstDayOfMonth(), selectedMonth.LastDayOfMonth());
            
            return _factory.GetExpensesState(this, specification, expenseAggregator, 
                new TableOptions(){Title = selectedMonth.ToString("MMMM yyyy"), ColumnNames = new []{"Category", "AMD", "RUR"}});
        }

        return this;
    }
}