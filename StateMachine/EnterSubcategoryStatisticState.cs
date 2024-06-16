using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterSubcategoryStatisticState : IExpenseInfoState
{
    private readonly Category _category;
    private SubCategory? _subCategory;
    private readonly ILogger _logger;

    public EnterSubcategoryStatisticState(IExpenseInfoState previousState, Category category,
        ILogger logger)
    {
        PreviousState = previousState;
        _category = category;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var buttons = TelegramKeyboard.FromButtons(_category.SubCategories.Select(sc => new TelegramButton(){CallbackData = sc.Name, Text = sc.Name}));

        return await botClient.SendTextMessageAsync(chatId, "Choose a subcategory", keyboard: buttons, cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => _subCategory = _category.SubCategories.FirstOrDefault(c => c.Name == message.Text));
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        return _subCategory != null
            ? stateFactory.CreateCollectSubcategoryExpensesByMonthsState(this, _category, _subCategory)
            : this;
    }
}