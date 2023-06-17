using System;
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
            await botClient.SendTextMessageAsync(chatId: chatId, "Collecting expenses... It can take some time.");

            var rows = await _spreadsheetWrapper.GetRows(_dateFilter, _logger, cancellationToken);
            rows = rows.Where(expense => _categoryFilter(expense.Category)).ToList();

            _logger.LogInformation($"Found {rows.Count} row(s).");

            Message lastMessage = default;
            foreach (var currency in new[] {Currency.Amd, Currency.Rur})
            {
                var (categories, total) = _expensesAggregator.Aggregate(rows, currency);
                string[,] telegramTable = new string[categories.Count + 2, 2];
                int i = 0;
                foreach ((string category, Money sum) in categories)
                {
                    telegramTable[i, 0] = category;
                    telegramTable[i, 1] = sum.ToString();
                    i++;
                }

                telegramTable[categories.Count, 0] = "";
                telegramTable[categories.Count, 1] = "";

                telegramTable[categories.Count + 1, 0] = "Всего";
                telegramTable[categories.Count + 1, 1] = total.ToString();

                var table = MarkdownFormatter.FormatTable(_tableOptions, telegramTable);

                if (total.Amount > 0)
                {
                    lastMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                        $"```{TelegramEscaper.EscapeString(table)}```",
                        cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
                }
            }

            if (lastMessage == default)
            {
                lastMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                    $"There is no any expenses for this period",
                    cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
            }

            return lastMessage;
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            return _factory.CreateGreetingState();
        }
    }
}