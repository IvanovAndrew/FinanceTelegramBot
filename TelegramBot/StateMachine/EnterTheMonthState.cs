using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.StateMachine;

internal class EnterTheMonthState : IExpenseInfoState
{
    private static readonly string[] MonthNames = {
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November",
        "December"
    };
    public bool UserAnswerIsRequired => true;
    private readonly StateFactory _factory;
    private readonly DateOnly _today;
    private readonly ILogger _logger;
    private readonly string _dateFormat = "yyyy-MM-dd";
    
    public IExpenseInfoState PreviousState { get; private set; }

    public EnterTheMonthState(StateFactory factory, IExpenseInfoState previousState, DateOnly today, ILogger logger)
    {
        _factory = factory;
        _today = today;
        _logger = logger;
        PreviousState = previousState;
    }
    
    public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var info = "Enter the month";

        var buttons = new[]
            {
                _today.AddMonths(-5), _today.AddMonths(-4), _today.AddMonths(-3),
                _today.AddMonths(-2), _today.AddMonths(-1), _today 
            }.Chunk(3)
            .Select(row => row.Select(date => InlineKeyboardButton.WithCallbackData(text: $"{MonthNames[date.Month - 1]} {date.Year}", callbackData: date.ToString(_dateFormat))))
            .ToList();
        
        InlineKeyboardMarkup inlineKeyboard = new(buttons);
        
        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: info,
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
    {
        if (DateOnly.TryParseExact(text, _dateFormat, out var selectedMonth))
        {
            return _factory.GetMonthExpensesState(this, selectedMonth);
        }

        return this;
    }
}