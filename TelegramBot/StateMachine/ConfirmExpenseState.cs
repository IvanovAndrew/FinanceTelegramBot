using System;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine
{
    class ConfirmExpenseState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly IExpense _expense;
        private readonly ILogger _logger;
        public IExpenseInfoState PreviousState { get; private set; }
        public bool UserAnswerIsRequired => true;
    
        public ConfirmExpenseState(StateFactory stateFactory, IExpenseInfoState previousState, IExpense expense, ILogger logger)
        {
            _factory = stateFactory;
            _expense = expense;
            _logger = logger;
            PreviousState = previousState;
        }

    

        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            string infoMessage = string.Join($"{Environment.NewLine}", 
                $"Date: {_expense.Date:dd.MM.yyyy}", 
                $"Category: {_expense.Category}", 
                $"SubCategory: {_expense.SubCategory ?? string.Empty}", 
                $"Description: {_expense.Description ?? string.Empty}",
                $"Amount: {_expense.Amount}",
                "",
                "Would you like to save it?"
            );

            InlineKeyboardMarkup inlineKeyboard = new(
                // keyboard
                new[]
                {
                    // first row
                    new[]
                    {
                        // first button in row
                        InlineKeyboardButton.WithCallbackData(text: "Save", callbackData: "Save"),
                        InlineKeyboardButton.WithCallbackData(text: "Cancel", callbackData: "Cancel"),
                    }
                });

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"{infoMessage}",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            if (string.Equals(text, "Save", StringComparison.InvariantCultureIgnoreCase))
            {
                return _factory.CreateSaveState(this, _expense);
            }
            
            if (string.Equals(text, "Cancel", StringComparison.InvariantCultureIgnoreCase))
            {
                return _factory.CreateCancelState();
            }

            throw new ArgumentOutOfRangeException(nameof(text), $@"Expected values are ""Save"" or ""Cancel"". {text} was received.");
        }
    }
}