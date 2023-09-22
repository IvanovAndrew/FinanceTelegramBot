using Domain;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CollectExpensesByCategoryState<T> : IExpenseInfoState, ILongTermOperation
    {
        private readonly StateFactory _factory;
        private readonly ISpecification<IExpense> _specification;
        private readonly ExpensesAggregator<T> _expensesAggregator;
        private readonly TableOptions _tableOptions;
        private readonly IExpenseRepository _expenseRepository;
        private readonly Func<T, string> _firstColumnName;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        public IExpenseInfoState PreviousState { get; private set; }

        public CollectExpensesByCategoryState(StateFactory stateFactory, IExpenseInfoState previousState,
            ISpecification<IExpense> specification, ExpensesAggregator<T> expensesAggregator,
            Func<T, string> firstColumnName,
            TableOptions tableOptions,
            IExpenseRepository expenseRepository, ILogger logger)
        {
            _factory = stateFactory;
            _specification = specification;
            _expensesAggregator = expensesAggregator;
            _firstColumnName = firstColumnName;
            _tableOptions = tableOptions;
            _expenseRepository = expenseRepository;
            _logger = logger;
            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => false;

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            throw new InvalidCastException();
        }

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            // TODO move logic to here
            await Task.Run(() => { });
        }

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

        public IExpenseInfoState ToNextState(IMessage message, CancellationToken cancellationToken)
        {
            return _factory.CreateGreetingState();
        }

        public async Task<IMessage> Handle(ITelegramBot botClient, IMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Collecting expenses... It can take some time.");
            var collectingMessage = await botClient.SendTextMessageAsync(message.ChatId, "Collecting expenses... It can take some time.");

            string text = "";
            try
            {
                List<IExpense> expenses;
                using (_cancellationTokenSource = new CancellationTokenSource())
                {
                    expenses = await _expenseRepository.Read(_cancellationTokenSource.Token);
                }
                
                expenses = expenses.Where(expense => _specification.IsSatisfied(expense)).ToList();

                _logger.LogInformation($"{expenses.Count} expenses satisfy the requirements");
                
                if (expenses.Any())
                {
                    var currencies = new []{Currency.Amd, Currency.Rur, Currency.Gel };
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
                }
                else
                {
                    text = "There is no any expenses for this period";
                }
            }
            catch (OperationCanceledException e)
            {
                text = "Operation is canceled";
            }
            finally
            {
                _cancellationTokenSource = null;
                await botClient.DeleteMessageAsync(message.ChatId, collectingMessage.Id, cancellationToken);
            }
            
            return await botClient.SendTextMessageAsync(chatId: message.ChatId, text, useMarkdown:true, cancellationToken: cancellationToken);;
        }

        private Currency[] GetAvailableCurrencies(List<IExpense> expenses)
        {
            // TODO move to appsettings
            Dictionary<Currency, int> priorities = new Dictionary<Currency, int>()
            {
                [Currency.Amd] = 0,
                [Currency.Rur] = 1,
                [Currency.Gel] = 2,
            };
            
            return 
                expenses.Where(expense => expense.Amount.Amount != 0m).Select(expense => expense.Amount.Currency)
                    .OrderBy(c => priorities[c]).ToArray();
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}