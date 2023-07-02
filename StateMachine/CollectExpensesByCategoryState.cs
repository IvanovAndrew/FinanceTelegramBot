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
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        public IExpenseInfoState PreviousState { get; private set; }

        public CollectExpensesByCategoryState(StateFactory stateFactory, IExpenseInfoState previousState,
            ISpecification<IExpense> specification, ExpensesAggregator<T> expensesAggregator,
            TableOptions tableOptions,
            IExpenseRepository expenseRepository, ILogger logger)
        {
            _factory = stateFactory;
            _specification = specification;
            _expensesAggregator = expensesAggregator;
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
            var (amdCategories, amdTotal) = _expensesAggregator.Aggregate(rows, Currency.Amd);
            var (rurCategories, rurTotal) = _expensesAggregator.Aggregate(rows, Currency.Rur);
            
            var uniqueCategories1 = new HashSet<string>(amdCategories.Select(c => c.Item1));
            var uniqueCategories2 = new HashSet<string>(rurCategories.Select(c => c.Item1));
            
            uniqueCategories1.UnionWith(uniqueCategories2);
            string[,] telegramTable = new string[uniqueCategories1.Count + 2, 3];
            var zeroRur = new Money() { Amount = 0, Currency = Currency.Rur }.ToString("N0");
            int i = 0;
            foreach ((string category, Money sum) in amdCategories)
            {
                telegramTable[i, 0] = ShortNameOfCategory(category);
                telegramTable[i, 1] = sum.ToString("N0");

                var rur = rurCategories.FirstOrDefault(c => c.Item1 == category);
                
                telegramTable[i, 2] = rur.Item1 == category? rur.Item2.ToString("N0") : zeroRur;
                uniqueCategories2.Remove(category);
                i++;
            }

            var zeroAmd = new Money() { Amount = 0, Currency = Currency.Amd }.ToString("N0");
            foreach ((string category, Money sum) in rurCategories)
            {
                if (!uniqueCategories2.Contains(category)) continue;
                
                telegramTable[i, 0] = ShortNameOfCategory(category);
                telegramTable[i, 1] = zeroAmd;

                telegramTable[i, 2] = sum.ToString("N0");
                i++;
            }

            telegramTable[i, 0] = "";
            telegramTable[i, 1] = "";
            telegramTable[i, 2] = "";

            telegramTable[i + 1, 0] = "Total";
            telegramTable[i + 1, 1] = amdTotal.ToString("N0");
            telegramTable[i + 1, 2] = rurTotal.ToString("N0");

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