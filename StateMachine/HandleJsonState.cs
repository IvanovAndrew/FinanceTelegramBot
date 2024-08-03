using System.Net.Mime;
using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class HandleJsonState : IExpenseInfoState
{
    public bool UserAnswerIsRequired => false;
    private ITelegramFileInfo FileInfo { get; }
    private List<IExpense> _expenses = new();
    private readonly ILogger _logger;
        
    internal HandleJsonState(ITelegramFileInfo fileInfo, ILogger logger)
    {
        FileInfo = fileInfo;
        _logger = logger;
    }
        
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        var file = await botClient.GetFileAsync(FileInfo.FileId, FileInfo.MimeType, cancellationToken);

        if (string.IsNullOrEmpty(file?.Text))
        {
            return await botClient.SendTextMessageAsync(chatId, "Couldn't download the file or the file is empty", cancellationToken: cancellationToken);
        }

        FileInfo.Content = file;
        
        return await botClient.SendTextMessageAsync(chatId, "File downloaded", cancellationToken: cancellationToken);
    }

    public async Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        await Task.Run(() => _expenses = new ExpenseJsonParser().Parse(FileInfo.Content!.Text, "Еда", Currency.Rur));
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) => stateFactory.CreateRequestPasteJsonState();

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(FileInfo.Content?.Text))
        {
            return stateFactory.CreateErrorWithRetryState("Couldn't download the file or the file is empty", this);
        }

        if (_expenses.Count == 0)
        {
            return stateFactory.CreateErrorWithRetryState("File doesn't contain any expenses", this);
        }

        return stateFactory.CreateSaveExpensesFromJsonState(_expenses);
    }
}