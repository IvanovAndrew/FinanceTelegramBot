using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using GoogleSheetWriter;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.StateMachine
{
    internal class CollectExpensesByCategoryState<T> : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly Predicate<DateOnly> _dateFilter;
        private readonly Predicate<string> _categoryFilter;
        private readonly ExpensesAggregator<T> _expensesAggregator;
        private readonly TableOptions _tableOptions;
        private readonly GoogleSheetWrapper _spreadsheetWrapper;
        private readonly ILogger _logger;

        public IExpenseInfoState PreviousState { get; private set; }

        public CollectExpensesByCategoryState(StateFactory stateFactory, IExpenseInfoState previousState,
            Predicate<DateOnly> dateFilter, Predicate<string> categoryFilter, ExpensesAggregator<T> expensesAggregator,
            TableOptions tableOptions,
            GoogleSheetWrapper spreadsheetWrapper, ILogger logger)
        {
            _factory = stateFactory;
            _dateFilter = dateFilter;
            _categoryFilter = categoryFilter;
            _expensesAggregator = expensesAggregator;
            _tableOptions = tableOptions;
            _spreadsheetWrapper = spreadsheetWrapper;
            _logger = logger;
            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => false;

        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Collecting expenses... It can take some time.");
            var message = await botClient.SendTextMessageAsync(chatId: chatId, "Collecting expenses... It can take some time.");

            var rows = await _spreadsheetWrapper.GetRows(_dateFilter, _logger, cancellationToken);
            rows = rows.Where(expense => _categoryFilter(expense.Category)).ToList();

            _logger.LogInformation($"Found {rows.Count} row(s).");

            string text = "There is no any expenses for this period";
            var (amdCategories, amdTotal) = _expensesAggregator.Aggregate(rows, Currency.Amd);
            var (rurCategories, rurTotal) = _expensesAggregator.Aggregate(rows, Currency.Rur);
            
            var uniqueCategories1 = new HashSet<string>(amdCategories.Select(c => c.Item1));
            var uniqueCategories2 = new HashSet<string>(rurCategories.Select(c => c.Item1));
            
            uniqueCategories1.UnionWith(uniqueCategories2);
            string[,] telegramTable = new string[uniqueCategories1.Count + 2, 3];
            int i = 0;
            foreach ((string category, Money sum) in amdCategories)
            {
                telegramTable[i, 0] = ShortNameOfCategory(category);
                telegramTable[i, 1] = sum.ToString("N0");

                var rur = rurCategories.FirstOrDefault(c => c.Item1 == category);
                
                telegramTable[i, 2] = rur.Item1 == category? rur.Item2.ToString() : "";
                uniqueCategories2.Remove(category);
                i++;
            }

            foreach ((string category, Money sum) in rurCategories)
            {
                if (!uniqueCategories2.Contains(category)) continue;
                
                telegramTable[i, 0] = ShortNameOfCategory(category);
                telegramTable[i, 1] = "";

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

            if (amdTotal.Amount > 0 || rurTotal.Amount > 0)
            {
                text = $"```{TelegramEscaper.EscapeString(table)}```";
            }

            await botClient.DeleteMessageAsync(chatId, message.MessageId, cancellationToken);
            return await botClient.SendTextMessageAsync(chatId: chatId, text, cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);;
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
    }
}