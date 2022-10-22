using System.Collections.Concurrent;
using System.Collections.Generic;
using Domain;
using GoogleSheet;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.StateMachine;

namespace TelegramBot.Controllers;

[ApiController]
[Route(Services.TelegramBot.Route)]
public class BotController : ControllerBase
{
    private static ConcurrentDictionary<long, IExpenseInfoState> answers = new();

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
        
        if (text.ToLowerInvariant() == "/start")
        {
            await SendGreetingInline(botClient: botClient, chatId: chatId, cancellationToken: cancellationTokenSource.Token);
            return Ok();
        }
        else if (text.ToLowerInvariant() == "/cancel")
        {
            cancellationTokenSource.Cancel();
            answers.Remove(chatId, out var prevState);
            await botClient.SendTextMessageAsync(chatId, $"All operations are canceled");
            return Ok();
        }

        if (!answers.TryGetValue(chatId, out IExpenseInfoState state))
        {
            state = new EnterTheDateState(_categories, _moneyParser, _spreadsheetWriter, _logger);
            answers[chatId] = state;
            await state.Request(botClient, chatId, cancellationTokenSource.Token);
        }
        else
        {
            var newState = state.Handle(text, cancellationTokenSource.Token);
            if (newState != null)
            {
                answers[chatId] = newState;
                await newState.Request(botClient, chatId, cancellationTokenSource.Token);
            }
            else
            {
                answers.Remove(chatId, out var previousState);
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

    private static async Task<Message> SendGreetingInline(ITelegramBotClient botClient, long chatId,
        CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
                new[] { InlineKeyboardButton.WithCallbackData(text: "Enter the outcome", callbackData: "startExpense") }
            );

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "What should I do?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
}