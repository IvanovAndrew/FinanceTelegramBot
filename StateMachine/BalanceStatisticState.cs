using Domain;
using Infrastructure;
using Infrastructure.Telegram;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class BalanceStatisticState : IExpenseInfoState, ILongTermOperation
{
    private readonly FinanceFilter _financeFilter;
    private readonly TableOptions _tableOptions;
    private readonly IFinanceRepository _financeRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    
    public BalanceStatisticState(FinanceFilter financeFilter, IFinanceRepository financeRepository, ILogger<StateFactory> logger)
    {
        _financeFilter = financeFilter;
        _financeRepository = financeRepository;
        
        _tableOptions = new TableOptions()
            { Title = $"Balance from {_financeFilter.DateFrom.Value.ToString("Y")}", FirstColumnName = "Income", OtherColumns = new[] { "Outcome", "Saldo" } };
        
        _logger = logger;
    }

    public bool UserAnswerIsRequired => false;

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(new EditableMessageToSend(){ChatId = chatId, Text = "Wait"}, cancellationToken: cancellationToken);
    }

    public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
    {
        // TODO move logic to here
        return Task.CompletedTask;
    }

    public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) => stateFactory.CreateChooseStatisticState();

    public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
        CancellationToken cancellationToken)
    {
        return stateFactory.CreateGreetingState();
    }

    public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
    {
        bool tableFilled = false;
        string text = "";
        
        if (TelegramCommand.TryGetCommand(message.Text, out _))
        {
            await Cancel();
            text = "Canceled";
        }
        else
        {
            await botClient.SendTextMessageAsync(new EditableMessageToSend(){ChatId = message.ChatId, Text = "Collecting expenses... It can take some time."}, cancellationToken: cancellationToken);
        
            try
            {
                List<IMoneyTransfer> expenses;
                List<IMoneyTransfer> incomes;

                Task<List<IMoneyTransfer>>[] tasks; 
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    tasks = new[]
                    {
                        _financeRepository.ReadOutcomes(_financeFilter, _cancellationTokenSource.Token),
                        _financeRepository.ReadIncomes(_financeFilter, _cancellationTokenSource.Token)
                    };

                    var results = await Task.WhenAll(tasks);
                    expenses = results[0];
                    incomes = results[1];
                }
                
                _logger.LogInformation($"{expenses.Count} expenses satisfy the requirements");
                _logger.LogInformation($"{incomes.Count} incomes satisfy the requirements");
                
                if (expenses.Any())
                {
                    Money monthExpenses = new Money() { Amount = 0, Currency = _financeFilter.Currency?? Currency.Rur }; 
                    
                    foreach (var expense in expenses)
                    {
                        monthExpenses += expense.Amount;
                    }
                    
                    Money monthIncomes = new Money() { Amount = 0, Currency = _financeFilter.Currency?? Currency.Rur }; 
                    foreach (var income in incomes)
                    {
                        if (income.Amount.Currency != _financeFilter.Currency || income.Date < _financeFilter.DateFrom)
                            continue;
                        
                        monthIncomes += income.Amount;
                    }

                    var telegramTable = new string[1, 3];
                    telegramTable[0, 0] = $" {monthIncomes.ToString()} ";
                    telegramTable[0, 1] = $" {monthExpenses.ToString()} ";
                    telegramTable[0, 2] = $" {(monthIncomes - monthExpenses).ToString()} ";

                    text = MarkdownFormatter.FormatTable(_tableOptions, telegramTable);
                    tableFilled = true;
                }
                else
                {
                    text = "There is no any expenses for this period";
                }
            }
            catch (OperationCanceledException)
            {
                text = "Operation is canceled by a user";
                _logger.LogInformation(text);
            }
            finally
            {
                _cancellationTokenSource = null;
            }
        }
        
        _logger.LogInformation($"Sending text: {text} tableFilled = {tableFilled}");
        _logger.LogInformation("Passing the logger " + _logger + " to " + botClient.GetType());
        return await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = text, UseMarkdown = tableFilled}, cancellationToken: cancellationToken);
    }

        public Task Cancel()
        {
            return Task.Run(() =>
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            );
        }
}