using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine;

class BalanceStatisticState : IExpenseInfoState, ILongTermOperation
{
    private readonly FinanceFilter _financeFilter;
    private readonly TableOptions _tableOptions;
    private readonly IFinanseRepository _finanseRepository;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    
    private readonly DateOnly _today;

    public BalanceStatisticState(DateOnly today, IFinanseRepository finanseRepository, ILogger<StateFactory> logger)
    {
        _today = today;
        _logger = logger;
        _finanseRepository = finanseRepository;
        _financeFilter = new FinanceFilter() { Currency = Currency.Amd, DateFrom = today.FirstDayOfMonth()};
        _tableOptions = new TableOptions()
            { Title = "Balance", FirstColumnName = "Month", OtherColumns = new[] { "Income", "Outcome", "Saldo" } };
    }

    public bool UserAnswerIsRequired => false;

    public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
    {
        return await botClient.SendTextMessageAsync(chatId, "Wait", cancellationToken:cancellationToken);
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
            var collectingMessage = await botClient.SendTextMessageAsync(message.ChatId, "Collecting expenses... It can take some time.");
        
            try
            {
                List<IExpense> expenses;
                List<IIncome> incomes;
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    expenses = await _finanseRepository.ReadOutcomes(_financeFilter, _cancellationTokenSource.Token);
                    incomes = await _finanseRepository.ReadIncomes(_financeFilter, _cancellationTokenSource.Token);
                }
                
                _logger.LogInformation($"{expenses.Count} expenses satisfy the requirements");
                _logger.LogInformation($"{incomes.Count} incomes satisfy the requirements");
                
                if (expenses.Any())
                {
                    
                    Money monthExpenses = new Money() { Amount = 0, Currency = Currency.Amd }; 
                    
                    foreach (var expense in expenses)
                    {
                        if (expense.Amount.Currency != Currency.Amd)
                            continue;
                        
                        if (expense.Date.LastDayOfMonth() != _today.LastDayOfMonth())
                            continue;

                        monthExpenses += expense.Amount;
                    }
                    
                    Money monthIncomes = new Money() { Amount = 0, Currency = Currency.Amd }; 
                    foreach (var expense in incomes)
                    {
                        if (expense.Amount.Currency != Currency.Amd)
                            continue;
                        
                        if (expense.Date.LastDayOfMonth() != _today.LastDayOfMonth())
                            continue;

                        monthIncomes += expense.Amount;
                    }

                    var telegramTable = new string[1, 4];
                    telegramTable[0, 0] = _today.ToString("Y");
                    telegramTable[0, 1] = monthIncomes.ToString();
                    telegramTable[0, 2] = monthExpenses.ToString();
                    telegramTable[0, 3] = (monthIncomes - monthExpenses).ToString();

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
                text = "Operation is canceled by user";
            }
            finally
            {
                _cancellationTokenSource = null;
                await botClient.DeleteMessageAsync(collectingMessage, cancellationToken);
            }
        }
        
        return await botClient.SendTextMessageAsync(chatId: message.ChatId, text, useMarkdown:tableFilled, cancellationToken: cancellationToken);
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