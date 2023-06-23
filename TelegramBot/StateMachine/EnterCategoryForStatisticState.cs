using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.StateMachine;

namespace TelegramBot
{
    internal class EnterCategoryForStatisticState : IExpenseInfoState
    {
        private StateFactory _factory;
        private readonly IEnumerable<Category> _categories;
        private ILogger _logger;
        
        public EnterCategoryForStatisticState(StateFactory factory, IExpenseInfoState previousState, IEnumerable<Category> categories, ILogger logger)
        {
            _factory = factory;
            PreviousState = previousState;
            _logger = logger;
            _categories = categories;
        }

        public bool UserAnswerIsRequired { get; } = true;
        public IExpenseInfoState PreviousState { get; }
        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = "Enter the category";

            var rows = _categories.Chunk(4);
        
            InlineKeyboardMarkup inlineKeyboard = new(
                // keyboard
                rows.Select(row => row.Select(c => InlineKeyboardButton.WithCallbackData(text:c.Name, callbackData:c.Name))).ToArray()
            );
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: infoMessage,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            var categoryDomain = _categories.FirstOrDefault(c => c.Name == text);
            if (categoryDomain != null)
            {
                return _factory.GetEnterTypeOfCategoryStatistic(this, categoryDomain);
            }

            return this;
        }
    }

    internal class EnterTypeOfCategoryStatisticState : IExpenseInfoState
    {
        private StateFactory _factory;
        private readonly string _category;
        private ILogger _logger;
        public bool UserAnswerIsRequired => true;
        public IExpenseInfoState PreviousState { get; }
        
        public EnterTypeOfCategoryStatisticState(StateFactory factory, IExpenseInfoState previousState, string category, ILogger logger)
        {
            _factory = factory;
            _category = category;
            PreviousState = previousState;
            _logger = logger;
        }
        
        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var buttons = new[]
            {
                InlineKeyboardButton.WithCallbackData(text: $"Subcategory", callbackData: "subcategory"),
                InlineKeyboardButton.WithCallbackData(text: $"For last six month", callbackData: "lastsixmonth"), 
            };
        
            InlineKeyboardMarkup inlineKeyboard = new(buttons);
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            if (text == "subcategory")
            {
                var firstDayOfMonth = DateOnly.FromDateTime(DateTime.Today).FirstDayOfMonth();
                
                var expenseAggregator = new ExpensesAggregator<string>(
                    e => e.SubCategory?? string.Empty, s => s, true, sortAsc:false);
                return _factory.GetExpensesState(this, 
                    d => d >= firstDayOfMonth, 
                    c => string.Equals(c, _category), 
                    expenseAggregator, 
                    new TableOptions()
                    {
                        Title = $"Category: {_category}. Expenses from {firstDayOfMonth.ToString("dd MMMM yyyy")}",
                        ColumnNames = new []{"Subcategory", "AMD", "RUR"}
                    });
            }

            if (text == "lastsixmonth")
            {
                var expenseAggregator = new ExpensesAggregator<DateOnly>(
                    e => e.Date.LastDayOfMonth(), d => d.ToString("yyyy MMM"), false, sortAsc:true);
                return _factory.GetExpensesState(this, 
                    d => d >= DateOnly.FromDateTime(DateTime.Today.AddMonths(-5)).FirstDayOfMonth(), 
                    c => string.Equals(c, _category), expenseAggregator,
                    new TableOptions()
                    {
                        Title = $"Category: {_category}",
                        ColumnNames = new []{"Month", "AMD", "RUR"}
                    });
            }

            throw new ArgumentOutOfRangeException(text);
        }
    }
}