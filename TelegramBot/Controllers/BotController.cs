using System.Collections.Concurrent;
using Domain;
using GoogleSheet;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot;
using TelegramBot.Controllers;
using TelegramBot.Services;
using TelegramBot.StateMachine;

[ApiController]
[Route(TelegramBotService.Route)]
public class BotController : ControllerBase
{
    private static ConcurrentDictionary<long, IExpenseInfoState> _answers = new();
    private static ConcurrentDictionary<IExpenseInfoState, Message> _sentMessage = new();

    private readonly ILogger _logger;
    private readonly TelegramBotService _bot;
    private readonly IDateParser _dateParser;
    private readonly List<Category> _categories;
    private readonly IMoneyParser _moneyParser;
    private readonly GoogleSheetWrapper _spreadsheetWrapper;
    private readonly ConcurrentDictionary<long, CancellationTokenSource> _cancellationTokenSources;

    public BotController(ILogger<BotController> logger, TelegramBotService bot, CategoryOptions categoryOptions,
        IDateParser dateParser,
        IMoneyParser moneyParser, GoogleSheetWrapper spreadsheetWrapper)
    {
        _logger = logger;
        _bot = bot;
        _dateParser = dateParser;
        _categories = categoryOptions.Categories;
        _moneyParser = moneyParser;
        _spreadsheetWrapper = spreadsheetWrapper;
        _cancellationTokenSources = new ConcurrentDictionary<long, CancellationTokenSource>();
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        var botClient = await _bot.GetBot();
        
        long chatId = default;
        string? userText = null;

        if (update.Type == UpdateType.Message)
        {
            chatId = update.Message.Chat.Id;
            userText = update.Message.Text;
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            chatId = update.CallbackQuery.Message.Chat.Id;
            userText = update.CallbackQuery.Data;

            var selectedText = update.CallbackQuery.Message.ReplyMarkup.InlineKeyboard.SelectMany(c =>
                c.Select(b => b)).First(b => b.CallbackData == userText).Text;
            await botClient.SendTextMessageAsync(chatId, selectedText);
        }

        
        
        var cancellationTokenSource = GetCancellationTokenSource(chatId);
        
        await botClient.SetMyCommandsAsync(new List<BotCommand>()
        {
            new BotCommand { Command = "/start", Description = "Start"},
            new BotCommand { Command = "/back", Description = "Back"},
            new BotCommand { Command = "/cancel", Description = "Cancel"},
        }, scope: BotCommandScope.Default(), cancellationToken:cancellationTokenSource.Token);

        _logger.LogInformation($"{userText} was received");

        var text = userText!;

        if (text.ToLowerInvariant() == "/cancel" || text.ToLowerInvariant() == "отмена")
        {
            cancellationTokenSource.Cancel();
            _answers.Remove(chatId, out var prevState);
            await botClient.SendTextMessageAsync(chatId, $"All operations are canceled");
            return Ok();
        }
        
        var factory = new StateFactory(_dateParser, _categories, _moneyParser,
            _spreadsheetWrapper, _logger);

        if (!_answers.TryGetValue(chatId, out IExpenseInfoState state))
        {
            
            state = factory.CreateGreetingState();
            _answers[chatId] = state;
            var message = await state.Request(botClient, chatId, cancellationTokenSource.Token);
            _sentMessage[state] = message;
        }
        else
        {
            await RemovePreviousMessage(state, botClient, chatId, cancellationTokenSource);

            IExpenseInfoState newState;
            if (string.Equals(text, "/back"))
            {
                newState = state.PreviousState;
            }
            else
            {
                newState = state.Handle(text, cancellationTokenSource.Token);
            }

            _answers[chatId] = newState;
            
            var message = await newState.Request(botClient, chatId, cancellationTokenSource.Token);
            _sentMessage[newState] = message;
            
            if (!newState.UserAnswerIsRequired)
            {
                _answers[chatId] = newState = factory.CreateGreetingState();
                message = await newState.Request(botClient, chatId, cancellationTokenSource.Token);
                _sentMessage[newState] = message;
            }
        }

        return Ok();
    }

    private async Task RemovePreviousMessage(IExpenseInfoState state, TelegramBotClient botClient, long chatId,
        CancellationTokenSource cancellationTokenSource)
    {
        if (_sentMessage.TryRemove(state, out var previousMessage))
        {
            var diff = DateTime.Now.Subtract(previousMessage.Date);
            if (diff.Hours > 24)
            {
                _logger.LogWarning(
                    $"Couldn't delete message {previousMessage.MessageId} {previousMessage.Text} because it was sent less than 24 hours ago");
            }
            else
            {
                _logger.LogInformation($"Removing message {previousMessage.MessageId} \"{previousMessage.Text}\"");
                try
                {
                    await botClient.DeleteMessageAsync(chatId, previousMessage.MessageId, cancellationTokenSource.Token);
                    _logger.LogInformation($"Message {previousMessage.MessageId} \"{previousMessage.Text}\" is removed");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Couldn't delete message {previousMessage.MessageId} {previousMessage.Text}.", e);
                }
            }
        }
    }

    private CancellationTokenSource GetCancellationTokenSource(long chatId)
    {
        if (!_cancellationTokenSources.TryGetValue(chatId, out var cancellationTokenSource))
        {
            cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSources[chatId] = cancellationTokenSource;
        }

        return cancellationTokenSource;
    }
}