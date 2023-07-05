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
            _logger.LogInformation("Collecting expenses... It can take some time.");
            var message = await botClient.SendTextMessageAsync(chatId, "Collecting expenses... It can take some time.");

            List<IExpense> rows;
            using (_cancellationTokenSource = new CancellationTokenSource())
            {
                rows = await _expenseRepository.Read(_cancellationTokenSource.Token);
            }

            _cancellationTokenSource = null;
            
            rows = rows.Where(expense => _specification.IsSatisfied(expense)).ToList();

            _logger.LogInformation($"{rows.Count} expenses satisfy the requirements");

            string text = "There is no any expenses for this period";
            var currencies = new[] { Currency.Amd, Currency.Rur };
            var statistic = _expensesAggregator.Aggregate(rows, currencies);
            
            string[,] telegramTable = new string[statistic.Rows.Count + 2, 3];
            int i = 0;
            int column = 1;
            foreach (var expenseInfo in statistic.Rows)
            {
                telegramTable[i, 0] = ShortNameOfCategory(_firstColumnName(expenseInfo.Row));
                column = 1;
                foreach (var currency in currencies)
                {
                    telegramTable[i, column++] = expenseInfo[currency].ToString("N0");
                }
                
                i++;
            }

            telegramTable[i, 0] = "";
            telegramTable[i, 1] = "";
            telegramTable[i, 2] = "";

            telegramTable[i + 1, 0] = "Total";
            column = 1;
            foreach (var currency in currencies)
            {
                telegramTable[i, column++] = statistic.Total[currency].ToString("N0");
            }

            var table = MarkdownFormatter.FormatTable(_tableOptions, telegramTable);

            await botClient.DeleteMessageAsync(chatId, message.Id, cancellationToken);
            return await botClient.SendTextMessageAsync(chatId: chatId, table, useMarkdown:true, cancellationToken: cancellationToken);;
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

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            return _factory.CreateGreetingState();
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}