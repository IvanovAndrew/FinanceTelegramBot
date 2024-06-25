using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

public class EnterRawQrState : IExpenseInfoState
{
    private readonly IFnsService _fnsService;
    private readonly ILogger _logger;
    private FnsResponse? _check;
    
    public EnterRawQrState(IFnsService fnsService, ILogger logger)
    {
        _logger = logger;
        _fnsService = fnsService;
    }

    public bool UserAnswerIsRequired => true;
    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
    {
        return await botClient.SendTextMessageAsync(chatId, "Enter the string you get after QR reading", cancellationToken: cancellationToken);
    }

    public async Task Handle(IMessage message, CancellationToken cancellationToken)
    {
        _check = await _fnsService.GetCheck(message.Text);
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) =>
        stateFactory.WayOfEnteringExpenseState();

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
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