using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterTypeOfCategoryStatisticState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly Category _category;
    private readonly DateOnly _today;
    private ILogger _logger;
    
    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }

    public EnterTypeOfCategoryStatisticState(StateFactory factory, IExpenseInfoState previousState, Category category, DateOnly today,
        ILogger logger)
    {
        _factory = factory;
        _category = category;
        _today = today;
        PreviousState = previousState;
        _logger = logger;
        
    }

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = TelegramKeyboard.FromButtons(new[]
        {
            new TelegramButton() { Text = "Subcategory", CallbackData = "subcategory" },
            new TelegramButton() { Text = "By period", CallbackData = "periodtodate" },
            new TelegramButton() { Text = "Subcategory by period", CallbackData = "subcategoryperiodtodate" },
        });

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text:"Choose type",
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        if (message.Text == "subcategory")
        {
            return _factory.CreateCollectCategoryExpensesBySubcategoriesForAPeriodState(this, _category);
            
        }

        if (message.Text == "periodtodate")
        {
            return _factory.CreateCollectCategoryExpensesByMonthsState(this, _category);
        }
        
        if (message.Text == "subcategoryperiodtodate")
        {
            return _factory.EnterSubcategoryStatisticState(this, _category);
        }

        throw new BotStateException(new []{"subcategory", "lastyear", "subcategoryperiodtodate"}, message.Text);
    }
}