using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterTheMonthState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => true;
    private readonly StateFactory _factory;
    protected readonly DateOnly Today;
    private readonly ILogger _logger;
    protected const string DateFormat = "MMMM yyyy";

    public IExpenseInfoState PreviousState { get; private set; }

    public EnterTheMonthState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger)
    {
        _factory = factory;
        Today = today;
        _logger = logger;
        PreviousState = previousState;
    }
    
    public virtual async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        var info = "Enter the month";

        var monthAgo = Today.AddMonths(-1);

        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            new TelegramButton { Text = $"{Today.ToString(DateFormat)}", CallbackData = $"{Today.ToString(DateFormat)}" },
            new TelegramButton { Text = $"{monthAgo.ToString(DateFormat)}", CallbackData = $"{monthAgo.ToString(DateFormat)}" },
            new TelegramButton { Text = "Another month", CallbackData = "custom" },
        }, chunkSize: 3);
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: info,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => { });
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        if (message.Text == "custom")
        {
            return new EnterTheCustomMonthState(_factory, this, Today, _logger);
        }
        
        if (DateOnly.TryParseExact(message.Text, DateFormat, out var selectedMonth))
        {
            var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, sortAsc:false);

            var specification =
                new ExpenseFromDateRangeSpecification(selectedMonth.FirstDayOfMonth(), selectedMonth.LastDayOfMonth());
            
            return _factory.GetExpensesState(this, specification, expenseAggregator, s => s,
                new TableOptions(){Title = selectedMonth.ToString(DateFormat), FirstColumnName = "Category"});
        }

        return this;
    }
}

internal class EnterTheCustomMonthState : EnterTheMonthState
{
    public EnterTheCustomMonthState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger) : base(factory, previousState, today, logger)
    {
    }

    public override async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(
            chatId: chatId, $"Enter the month. Example: {Today.ToString(DateFormat)}", cancellationToken:cancellationToken);
    }
}