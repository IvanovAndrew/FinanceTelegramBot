using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterTypeOfCategoryStatisticState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly string _category;
    private ILogger _logger;
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }

    public EnterTypeOfCategoryStatisticState(StateFactory factory, IExpenseInfoState previousState, string category,
        ILogger logger)
    {
        _factory = factory;
        _category = category;
        PreviousState = previousState;
        _logger = logger;
    }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            new TelegramButton()
            {
                Text = "Subcategory",
                CallbackData = "subcategory"
            },
            new TelegramButton() { Text = "For last year", CallbackData = "lastyear" },
        });

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Choose",
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => { });
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        if (message.Text == "subcategory")
        {
            var firstDayOfMonth = DateOnly.FromDateTime(DateTime.Today).FirstDayOfMonth();

            var expenseAggregator = new ExpensesAggregator<string>(
                e => e.SubCategory ?? string.Empty, true, sortAsc: false);

            var specification = new MultipleSpecification(
                new ExpenseLaterThanSpecification(firstDayOfMonth),
                new ExpenseFromCategorySpecification(_category));
            
            return _factory.GetExpensesState(this, specification,
                expenseAggregator,
                s => s,
                new TableOptions()
                {
                    Title = $"Category: {_category}. {Environment.NewLine}" +
                            $"Expenses from {firstDayOfMonth.ToString("dd MMMM yyyy")}",
                    ColumnNames = new[] { "Subcategory", "AMD", "RUR", "GEL" }
                });
        }

        if (message.Text == "lastyear")
        {
            var expenseAggregator = new ExpensesAggregator<DateOnly>(
                e => e.Date.LastDayOfMonth(), false, sortAsc: true);

            var specification =
                new MultipleSpecification(
                    new ExpenseLaterThanSpecification(DateOnly.FromDateTime(DateTime.Today.AddYears(-1)).FirstDayOfMonth()),
                    new ExpenseFromCategorySpecification(_category)
                );


            return _factory.GetExpensesState(this, specification, expenseAggregator,
                s => s.ToString("MMM yyyy"),
                new TableOptions()
                {
                    Title = $"Category: {_category}",
                    ColumnNames = new[] { "Month", "AMD", "RUR", "GEL" }
                });
        }

        throw new BotStateException(new []{"subcategory", "lastyear"}, message.Text);
    }
}