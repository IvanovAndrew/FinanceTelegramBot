using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

internal class SaveExpenseState : IExpenseInfoState
{
    private readonly IExpense _expense;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly ILogger _logger;
    
    internal SaveExpenseState(IExpense expense, GoogleSheetWriter spreadsheetWriter, ILogger logger)
    {
        _expense = expense;
        _spreadsheetWriter = spreadsheetWriter;
        _logger = logger;
    }

    public bool UserAnswerIsRequired => false;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving... It can take some time.");
        await botClient.SendTextMessageAsync(chatId: chatId, "Saving... It can take some time.");
        await _spreadsheetWriter.WriteToSpreadsheet(_expense, cancellationToken);

        _logger.LogInformation("Saved");
        return await botClient.SendTextMessageAsync(chatId: chatId, "Saved");
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}