using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class RequestFnsDataState : IExpenseInfoState
{
    private FnsResponse? _check;
    private readonly ILogger _logger;
    private readonly IFnsService _fnsService;
    private readonly string _messageText;

    public RequestFnsDataState(IFnsService fnsService, string messageText, ILogger logger)
    {
        _logger = logger;
        _fnsService = fnsService;
        _messageText = messageText;
    }
    
    public bool UserAnswerIsRequired => false;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(chatId, "Requesting FNS data...", cancellationToken: cancellationToken);
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
    {
        return stateFactory.CreateCheckInfoState();
    }
    
    public async Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        _check = await _fnsService.GetCheck(_messageText);
    }

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory, CancellationToken cancellationToken)
    {
        if (_check == null)
        {
            return stateFactory.CreateErrorWithRetryState("Couldn't download the receipt", this);
        }

        var expenses = _check.Data?.Json?.Items?.Select(i => (IExpense) new Expense()
        {
            Amount = new Money
            {
                Amount = i.Sum / 100m,
                Currency = Currency.Rur
            },
            Date = DateOnly.FromDateTime(_check.Data.Json.DateTime),
            Description = i.Name,
            Category = "Еда"
        })?.ToList()?? new List<IExpense>();
        
        if (!expenses.Any())
        {
            return stateFactory.CreateErrorWithRetryState("The receipt doesn't contain any expenses", this);
        }

        return stateFactory.CreateSaveExpensesFromJsonState(expenses);
    }
}