using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

internal class EnterSubCategoryExpensesState : IExpenseInfoState
{
    private readonly StateFactory _stateFactory;
    private readonly Category _category;
    private readonly ILogger<StateFactory> _logger;
    private SubCategory? _subCategory;

    public EnterSubCategoryExpensesState(StateFactory stateFactory, IExpenseInfoState previousState, Category category, ILogger<StateFactory> logger)
    {
        _stateFactory = stateFactory;
        PreviousState = previousState;
        _category = category;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => true;
    public IExpenseInfoState PreviousState { get; }
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var keyboard = TelegramKeyboard.FromButtons(_category.SubCategories.Select(c => new TelegramButton()
            { Text = c.Name, CallbackData = c.Name }));

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            keyboard: keyboard,
            cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => _subCategory = _category.SubCategories.FirstOrDefault(c => c.Name == message.Text), cancellationToken);
    }

    public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
    {
        return _subCategory != null ? _stateFactory.CreateCollectSubCategoryExpensesState(this, _category, _subCategory!) : this;
    }
}