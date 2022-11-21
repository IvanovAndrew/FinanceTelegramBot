using System.Collections.Concurrent;
using Domain;
using GoogleSheet;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Controllers;
using TelegramBot.Services;
using TelegramBot.StateMachine;

[ApiController]
[Route(TelegramBotService.Route)]
public class BotController : ControllerBase
{
    private static ConcurrentDictionary<long, IExpenseInfoState> _answers = new();
    private static ConcurrentDictionary<IExpenseInfoState, Message> _sentMessage = new();

    private readonly ILogger<BotController> _logger;
    private readonly TelegramBotService _bot;
    private readonly List<Category> _categories;
    private readonly IMoneyParser _moneyParser;
    private readonly GoogleSheetWriter _spreadsheetWriter;
    private readonly ConcurrentDictionary<long, CancellationTokenSource> _cancellationTokenSources;

    public BotController(ILogger<BotController> logger, TelegramBotService bot, CategoryOptions categoryOptions,
        IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter)
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

        _logger.LogInformation($"{userText} was received");

        var text = userText!;

        if (text.ToLowerInvariant() == "/cancel")
        {
            cancellationTokenSource.Cancel();
            _answers.Remove(chatId, out var prevState);
            await botClient.SendTextMessageAsync(chatId, $"All operations are canceled");
            return Ok();
        }

        if (!_answers.TryGetValue(chatId, out IExpenseInfoState state))
        {
            state = new GreetingState(DateOnly.FromDateTime(DateTime.Today), _categories, _moneyParser, _spreadsheetWriter, _logger);
            _answers[chatId] = state;
            var message = await state.Request(botClient, chatId, cancellationTokenSource.Token);
            _sentMessage[state] = message;
        }
        else
        {
            if (_sentMessage.TryGetValue(state, out var previousMessage))
            {
                _logger.LogInformation($"Removing message {previousMessage.MessageId} {previousMessage.Text}");
                await botClient.DeleteMessageAsync(chatId, previousMessage.MessageId, cancellationTokenSource.Token);
                _logger.LogInformation($"Message {previousMessage.MessageId} {previousMessage.Text} is removed");
            }
            
            var newState = _answers[chatId] = state.Handle(text, cancellationTokenSource.Token);
            
            var message = await newState.Request(botClient, chatId, cancellationTokenSource.Token);
            _sentMessage[newState] = message;
            
            if (!newState.UserAnswerIsRequired)
            {
                _answers[chatId] = newState = new GreetingState(DateOnly.FromDateTime(DateTime.Today), _categories,
                    _moneyParser, _spreadsheetWriter, _logger);
                message = await newState.Request(botClient, chatId, cancellationTokenSource.Token);
                _sentMessage[newState] = message;
            }
        }

        return Ok();
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