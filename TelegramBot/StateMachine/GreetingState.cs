using Domain;
using GoogleSheet;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine;

class GreetingState : IExpenseInfoState
{
    private StateFactory _factory;
    private readonly ILogger _logger;
    
    public IExpenseInfoState PreviousState { get; private set; }
    
    public GreetingState(StateFactory factory, ILogger logger)
    {
        _factory = factory;
        _logger = logger;
        PreviousState = this;
    }

    public bool UserAnswerIsRequired => true;

    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Outcome", callbackData: "startExpense"),
                InlineKeyboardButton.WithCallbackData(text: "Statistics", callbackData: "showExpenses"),
            }
        );

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "What would you like to do?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        return text == "showExpenses" ? 
            _factory.CreateChooseStatisticState(this) : 
            _factory.CreateEnterTheDateState(this);
    }
}