using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

internal class SaveExpenseState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly IExpense _expense;
    private readonly GoogleSheetWrapper _spreadsheetWrapper;
    private readonly ILogger _logger;
    
    public IExpenseInfoState PreviousState { get; private set; }
    
    internal SaveExpenseState(StateFactory factory, IExpenseInfoState previousState, IExpense expense, GoogleSheetWrapper spreadsheetWrapper, ILogger logger)
    {
        _factory = factory;
        _expense = expense;
        _spreadsheetWrapper = spreadsheetWrapper;
        _logger = logger;

        PreviousState = previousState;
    }

    public bool UserAnswerIsRequired => false;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving... It can take some time.");
        await botClient.SendTextMessageAsync(chatId: chatId, "Saving... It can take some time.");
        await _spreadsheetWrapper.Write(_expense, cancellationToken);

        _logger.LogInformation("Saved");
        return await botClient.SendTextMessageAsync(chatId: chatId, "Saved");
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }
}