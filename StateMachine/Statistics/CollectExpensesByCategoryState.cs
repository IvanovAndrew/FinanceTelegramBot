using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CollectExpensesByCategoryState<T> : IExpenseInfoState, ILongTermOperation
    {
        private readonly FinanceFilter _financeFilter;
        private readonly ExpensesAggregator<T> _expensesAggregator;
        private readonly TableOptions _tableOptions;
        private readonly IFinanseRepository _finanseRepository;
        private readonly Func<T, string> _firstColumnName;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly IExpenseInfoState _previousState;

        public CollectExpensesByCategoryState(IExpenseInfoState previousState, FinanceFilter financeFilter, ExpensesAggregator<T> expensesAggregator,
            Func<T, string> firstColumnName,
            TableOptions tableOptions,
            IFinanseRepository finanseRepository, ILogger logger)
        {
            _previousState = previousState;
            _financeFilter = financeFilter;
            _expensesAggregator = expensesAggregator;
            _firstColumnName = firstColumnName;
            _tableOptions = tableOptions;
            _finanseRepository = finanseRepository;
            _logger = logger;
        }

        public bool UserAnswerIsRequired => false;

        public Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        public Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            // TODO move logic to here
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory) => _previousState;

        private static string ShortNameOfCategory(string name)
        {
            if (name == "Домашние животные")
                return "Коты";

            if (name == "Здоровье, гигиена")
                return "Здоровье";

            if (name == "Культурная жизнь")
                return "Развлечения";

            if (name == "Онлайн-сервисы")
                return "Сервисы";

            return name;
        }

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
                    using (_cancellationTokenSource = new CancellationTokenSource())
                    {
                        expenses = await _finanseRepository.ReadOutcomes(_financeFilter, _cancellationTokenSource.Token);
                    }
                    
                    _logger.LogInformation($"{expenses.Count} expenses satisfy the requirements");
                    
                    if (expenses.Any())
                    {
                        var currencies = expenses.Select(c => c.Amount.Currency).Distinct().ToArray();
                        _tableOptions.OtherColumns = currencies.Select(c => c.ToString()).ToArray();
                        var statistic = _expensesAggregator.Aggregate(expenses, currencies);
                
                        var telegramTable = new TelegramTableBuilder(statistic.Rows.Count + 2, currencies.Length + 1);
                        int i = 0;
                        
                        foreach (var expenseInfo in statistic.Rows)
                        {
                            telegramTable.FillRow(ShortNameOfCategory(_firstColumnName(expenseInfo.Row)), expenseInfo, currencies);
                            i++;
                        }

                        telegramTable.FillRow(string.Empty, string.Empty);
                        telegramTable.FillRow("Total", statistic.Total, currencies);

                        text = MarkdownFormatter.FormatTable(_tableOptions, telegramTable.Build());
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
}