using Domain;
using Infrastructure;
using Infrastructure.Fns;
using Infrastructure.Fns.DataContract;
using Infrastructure.Telegram;
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
        return await botClient.SendTextMessageAsync(new EditableMessageToSend(){ChatId = chatId, Text = "Requesting FNS data..."}, cancellationToken: cancellationToken);
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

        var expenses = _check.Data?.Json?.Items?.Select(i => (IMoneyTransfer) new Outcome()
        {
            Amount = new Money
            {
                Amount = i.Sum / 100m,
                Currency = Currency.Rur
            },
            Date = GetDate(_check.Data.Json.DateTime),
            Description = i.Name,
            Category = "Еда"
        })?.ToList()?? new List<IMoneyTransfer>();
        
        if (!expenses.Any())
        {
            return stateFactory.CreateErrorWithRetryState("The receipt doesn't contain any expenses", this);
        }

        return stateFactory.CreateSaveExpensesFromJsonState(expenses);
    }

    private DateOnly GetDate(DateTime date)
    {
        return DateOnly.FromDateTime(date.Hour < 4? date.AddDays(-1) : date);
    }
}