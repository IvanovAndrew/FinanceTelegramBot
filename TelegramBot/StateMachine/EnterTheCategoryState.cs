using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using GoogleSheet;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine;

class EnterTheCategoryState : IExpenseInfoState
{
    private readonly StateFactory _factory;
    private readonly ExpenseBuilder _expenseBuilder;
    private readonly IEnumerable<Category> _categories;
    private readonly ILogger _logger;
    
    public IExpenseInfoState PreviousState { get; private set; }

    internal EnterTheCategoryState(StateFactory stateFactory, IExpenseInfoState previousState, ExpenseBuilder expenseBuilder, IEnumerable<Category> categories, ILogger logger)
    {
        _factory = stateFactory;
        _expenseBuilder = expenseBuilder;
        _categories = categories;
        _logger = logger;

        PreviousState = previousState;
    }

    public bool UserAnswerIsRequired => true;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        string infoMessage = "Enter the category";

        var rows = _categories.Chunk(4);
        
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
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
            _expenseBuilder.Category = categoryDomain;
            if (categoryDomain.SubCategories.Any())
            {
                return _factory.CreateEnterTheSubcategoryState(_expenseBuilder, this, categoryDomain.SubCategories);
            }
            else
            {
                return _factory.CreateEnterDescriptionState(_expenseBuilder, this);
            }
        }

        return this;
    }
}