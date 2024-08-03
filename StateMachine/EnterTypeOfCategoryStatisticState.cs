using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterTypeOfCategoryStatisticState : IExpenseInfoState
{
    private readonly Category _category;
    private readonly DateOnly _today;
    private readonly IExpenseInfoState _previousState;
    private ILogger _logger;
    
    public bool UserAnswerIsRequired => true;
    public EnterTypeOfCategoryStatisticState(Category category, DateOnly today, IExpenseInfoState previousState, ILogger logger)
    {
        _category = category;
        _today = today;
        _previousState = previousState;
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

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return _previousState;
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        if (message.Text == "subcategory")
        {
            return stateFactory.CreateCollectCategoryExpensesBySubcategoriesForAPeriodState(_category);
        }

        if (message.Text == "periodtodate")
        {
            return stateFactory.CreateCollectCategoryExpensesByMonthsState(_category);
        }
        
        if (message.Text == "subcategoryperiodtodate")
        {
            return stateFactory.EnterSubcategoryStatisticState(this, _category);
        }

        throw new BotStateException(new []{"subcategory", "lastyear", "subcategoryperiodtodate"}, message.Text);
    }
}