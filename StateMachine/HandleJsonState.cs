using System.Net.Mime;
using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class HandleJsonState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    public bool UserAnswerIsRequired => false;
    public IExpenseInfoState PreviousState { get; }
    private ITelegramFileInfo FileInfo { get; }
    private readonly ILogger _logger;
        
    internal HandleJsonState(StateFactory factory, IExpenseInfoState previousState, ITelegramFileInfo fileInfo, ILogger logger)
    {
        _factory = factory;
        FileInfo = fileInfo;
        _logger = logger;
        PreviousState = previousState;
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

    public IExpenseInfoState Handle(IMessage message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(FileInfo.Content?.Text))
        {
            return _factory.CreateErrorWithRetryState("Couldn't download the file or the file is empty", this);
        }

        var parser = new ExpenseJsonParser();
        var expenses = parser.Parse(FileInfo.Content!.Text, "Еда", Currency.Rur);

        if (expenses.Count == 0)
        {
            return _factory.CreateErrorWithRetryState("File doesn't contain any expenses", this);
        }

        return _factory.CreateSaveExpensesFromJsonState(this, expenses);
    }
}