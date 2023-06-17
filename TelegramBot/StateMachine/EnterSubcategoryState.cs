using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine
{
    class EnterSubcategoryState : IExpenseInfoState
    {
        private readonly StateFactory _factory;
        private readonly SubCategory[] _subCategories;
        private readonly ExpenseBuilder _expenseBuilder;
        private readonly ILogger _logger;
    
        public IExpenseInfoState PreviousState { get; private set; }
    
        internal EnterSubcategoryState(StateFactory factory, IExpenseInfoState previousState, ExpenseBuilder builder, SubCategory[] subCategories, ILogger logger)
        {
            _factory = factory;
            _subCategories = subCategories;
            _expenseBuilder = builder;
            _logger = logger;
            PreviousState = previousState;
        }

        public bool UserAnswerIsRequired => true;

        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            var firstRow = _subCategories.Take(4);
            var secondRow = Enumerable.Empty<SubCategory>();
            if (_subCategories.Length > 4)
            {
                secondRow = _subCategories.Skip(4).Take(4);
            }    
        
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
                // keyboard
                new[]
                {
                    // first row
                    firstRow.Select(c => InlineKeyboardButton.WithCallbackData(text:c.Name, callbackData:c.Name)).ToArray(),
                    secondRow.Select(c => InlineKeyboardButton.WithCallbackData(text:c.Name, callbackData:c.Name)).ToArray(),
                });

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose the subcategory",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            var subCategory = _subCategories.FirstOrDefault(c => c.Name == text);
            if (subCategory != null)
            {
                _expenseBuilder.SubCategory = subCategory;
                return _factory.CreateEnterDescriptionState(_expenseBuilder, this);
            }

            return this;
        }

        public bool AnswerIsRequired => true;
    }
}