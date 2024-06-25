using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterSubcategoryStatisticState : IExpenseInfoState
{
    private readonly Category _category;
    private readonly IExpenseInfoState _previousState;
    private SubCategory? _subCategory;
    private readonly ILogger _logger;

    public EnterSubcategoryStatisticState(Category category, IExpenseInfoState previousState, ILogger logger)
    {
        _category = category;
        _previousState = previousState;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var buttons = TelegramKeyboard.FromButtons(_category.SubCategories.Select(sc => new TelegramButton(){CallbackData = sc.Name, Text = sc.Name}));

        return await botClient.SendTextMessageAsync(chatId, "Choose a subcategory", keyboard: buttons, cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => _subCategory = _category.SubCategories.FirstOrDefault(c => c.Name == message.Text));
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return _previousState;
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        return _subCategory != null
            ? stateFactory.CreateCollectSubcategoryExpensesByMonthsState(_category, _subCategory)
            : this;
    }
}