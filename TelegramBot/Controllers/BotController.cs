using System.Collections.Concurrent;
using Domain;
using GoogleSheet;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.StateMachine;

namespace TelegramBot.Controllers;

[ApiController]
[Route(Services.TelegramBot.Route)]
public class BotController : ControllerBase
{
    private static ConcurrentDictionary<long, IExpenseInfoState> answers = new();
    private static ConcurrentDictionary<IExpenseInfoState, Message> _sentMessage = new();

    private readonly ILogger<BotController> _logger;
    private readonly Services.TelegramBot _bot;
    private readonly List<Category> _categories;
    private readonly IMoneyParser _moneyParser;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly ConcurrentDictionary<long, CancellationTokenSource> _cancellationTokenSources;

    public BotController(ILogger<BotController> logger, Services.TelegramBot bot, CategoryOptions categoryOptions, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter)
    {
        _logger = logger;
        _bot = bot;
        _categories = categoryOptions.Categories;
        _moneyParser = moneyParser;
        _spreadsheetWriter = spreadsheetWriter;
        _cancellationTokenSources = new ConcurrentDictionary<long, CancellationTokenSource>();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        var cancellationTokenSource = GetCancellationTokenSource(update);
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
        }

        _logger.LogInformation($"{userText} was received");
        
        var text = userText!;
        
        if (text.ToLowerInvariant() == "/cancel")
        {
            cancellationTokenSource.Cancel();
            answers.Remove(chatId, out var prevState);
            await botClient.SendTextMessageAsync(chatId, $"All operations are canceled");
            return Ok();
        }

        if (!answers.TryGetValue(chatId, out IExpenseInfoState state))
        {
            state = new GreetingState(DateOnly.FromDateTime(DateTime.Today), _categories, _moneyParser, _spreadsheetWriter, _logger);
            answers[chatId] = state;
            var message = await state.Request(botClient, chatId, cancellationTokenSource.Token);
            _sentMessage[state] = message;
        }
        else
        {
            var newState = answers[chatId] = state.Handle(text, cancellationTokenSource.Token);
            
            var message = await newState.Request(botClient, chatId, cancellationTokenSource.Token);
            _sentMessage[newState] = message;
            
            if (!newState.UserAnswerIsRequired)
            {
                answers[chatId] = newState = new GreetingState(DateOnly.FromDateTime(DateTime.Today), _categories,
                    _moneyParser, _spreadsheetWriter, _logger);
                message = await newState.Request(botClient, chatId, cancellationTokenSource.Token);
                _sentMessage[newState] = message;
            }
            else
            {
                if (_sentMessage.TryGetValue(state, out var previousMessage))
                {
                    _logger.LogInformation($"Removing message {previousMessage.MessageId} {previousMessage.Text}");
                    await botClient.DeleteMessageAsync(chatId, previousMessage.MessageId);
                }
            }
        }

        return Ok();
    }

    private CancellationTokenSource GetCancellationTokenSource(Update update)
    {
        long senderId = default;
        if (update.Type == UpdateType.Message)
        {
            senderId = update.Message.From.Id;
        } 
        else if (update.Type == UpdateType.CallbackQuery)
        {
            senderId = update.CallbackQuery.Message.From.Id;
        }

        if (!_cancellationTokenSources.TryGetValue(senderId, out var cancellationTokenSource))
        {
            cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSources[senderId] = cancellationTokenSource;
        }

        return cancellationTokenSource;
    }
}