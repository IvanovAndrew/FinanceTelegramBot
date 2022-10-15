using System.Collections.Concurrent;
using System.Globalization;
using Domain;
using GoogleSheet;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Controllers;

[ApiController]
[Route(Services.TelegramBot.Route)]
public class BotController : ControllerBase
{
    private static ConcurrentDictionary<long, ExpenseBuilder> answers = new();

    private readonly ILogger<BotController> _logger;
    private readonly Services.TelegramBot _bot;
    private readonly List<Category> _categories;
    private readonly IMoneyParser _moneyParser;
    private readonly GoogleSheetWriter _spreadsheetWriter;

    public BotController(ILogger<BotController> logger, Services.TelegramBot bot, CategoryOptions categoryOptions, IMoneyParser moneyParser, GoogleSheetWriter spreadsheetWriter)
    {
        _logger = logger;
        _bot = bot;
        _categories = categoryOptions.Categories;
        _moneyParser = moneyParser;
        _spreadsheetWriter = spreadsheetWriter;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update)
    {
        var cancellationToken = new CancellationToken(false);
        var botClient = await _bot.GetBot();

        if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            _logger.LogInformation($"{message.Text} was received");
            if (message.Text.ToLower() == "/start")
            {
                await SendGreetingInline(botClient: botClient, chatId: message.Chat.Id,
                    cancellationToken: cancellationToken);
                return Ok();
            }
            else if (message.Text.ToLower() == "/cancel")
            {
                answers.Clear();
                await botClient.SendTextMessageAsync(message.Chat, $"All operations are canceled");
                return Ok();
            }

            if (answers.IsEmpty) return Ok();

            var builder = answers[message.From.Id];
            if (builder.Date == null)
            {
                DateOnly date;
                if (string.Equals("today", message.Text.Trim(), StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals("сегодня", message.Text.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    date = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    await botClient.SendTextMessageAsync(message.Chat, $"Today is {date}");
                }
                else if (string.Equals("yesterday", message.Text.Trim(), StringComparison.InvariantCultureIgnoreCase) ||
                    string.Equals("вчера", message.Text.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    var dateTime = DateTime.Now.AddDays(-1);
                    date = new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
                    await botClient.SendTextMessageAsync(message.Chat, $"Yesterday is {date}");
                }
                else if (!DateOnly.TryParse(message.Text, out date))
                {
                    _logger.LogDebug($"{message.Text} isn't a date");
                    await botClient.SendTextMessageAsync(message.Chat, $"{message.Text} isn't a date. Try again");
                    return Ok();
                }
                builder.Date = date;
                string infoMessage = "Enter the category";

                await SendCategoriesInline(botClient, message.Chat.Id, infoMessage, cancellationToken);
                return Ok();
            }
            else if (builder.Description == null)
            {
                builder.Description = message.Text;
                await RequestPrice(botClient, message.Chat.Id);
            }
            else if (builder.Sum == null)
            {
                if (!_moneyParser.TryParse(message.Text, out var money))
                {
                    string warning = $"{message.Text} wasn't recognized as money.";
                    _logger.LogWarning(warning);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"{warning} Try again", cancellationToken: cancellationToken);

                    return Ok();
                }

                builder.Sum = money;

                string s = string.Join(", ", 
                    $"{builder.Date.Value:dd.MM.yyyy}", 
                    $"{builder.Category}", 
                    $"{builder.SubCategory ?? string.Empty}", 
                    $"{builder.Description ?? string.Empty}",
                    $"{builder.Sum}"
                );
                string infoMessage = $"Check your data: {s}";
                await SendConfirmMessageAsync(botClient, message.Chat.Id, infoMessage, cancellationToken);
            }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            string codeOfButton = update.CallbackQuery.Data;
            var message = update.CallbackQuery.Message;
            
            _logger.LogInformation($"{codeOfButton} was received");
            
            long fromId = update.CallbackQuery.From.Id;

            if (!answers.TryGetValue(fromId, out ExpenseBuilder builder))
            {
                builder = new ExpenseBuilder();
            }
            
            if (codeOfButton == "startExpense")
            {
                answers[fromId] = builder;

                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Enter the date:",
                    parseMode: ParseMode.Html);
            }

            else if (builder != null && builder.Category == null && _categories.Any(c => c.Name == codeOfButton))
            {
                var category = message.ReplyMarkup.InlineKeyboard.SelectMany(c => c)
                    .First(c => c.CallbackData == codeOfButton);
                builder.Category = category.Text;

                var categoryDomain = _categories.First(c => c.Name == codeOfButton);
                if (categoryDomain.SubCategories.Any())
                {
                    await SendSubCategoriesInline(botClient, message.Chat.Id, categoryDomain, cancellationToken);
                }
                else
                {
                    await RequestDescription(botClient, message.Chat.Id);
                }
            }
            else if (builder != null && builder.Category != null && _categories.First(c => c.Name == builder.Category).SubCategories.Any(c => c.Name == codeOfButton))
            {
                var subCategory = _categories.First(c => c.Name == builder.Category).SubCategories.First(c => c.Name == codeOfButton);
                builder.SubCategory = subCategory.Name;

                await RequestDescription(botClient, message.Chat.Id);
            }
            
            else if (builder != null && codeOfButton == "Save")
            {
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Saving... It can take some time.");
                var expense = builder.Build();
                await _spreadsheetWriter.WriteToSpreadsheet(expense, cancellationToken);
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Saved");
                answers.Remove(fromId, out var removedBuilder);
            }
            
            else if (codeOfButton == "Cancel")
            {
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Canceled",
                    parseMode: ParseMode.Html);
                answers.Remove(fromId, out var removedBuilder);
            }
        }

        return Ok();
    }

    private static async Task<Message> SendGreetingInline(ITelegramBotClient botClient, long chatId,
        CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
            // keyboard
            new[]
            {
                // first row
                new[]
                {
                    // first button in row
                    InlineKeyboardButton.WithCallbackData(text: "Enter the outcome", callbackData: "startExpense"),
                },

            });

        return await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "What should I do?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    private async Task<Message> SendCategoriesInline(ITelegramBotClient botClient, long chatId, string text,
        CancellationToken cancellationToken)
    {
        var firstRow = _categories.Take(4);
        var secondRow = _categories.Skip(4).Take(4);
        
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
            text: text,
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    private async Task<Message> SendSubCategoriesInline(ITelegramBotClient botClient, long chatId, Category category,
        CancellationToken cancellationToken)
    {
        var firstRow = category.SubCategories.Take(4);
        var secondRow = Enumerable.Empty<SubCategory>();
        if (category.SubCategories.Length > 4)
        {
            secondRow = category.SubCategories.Skip(4).Take(4);
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

    private async Task<Message> RequestDescription(ITelegramBotClient botClient, long chatId)
    {
        return await botClient.SendTextMessageAsync(chatId, "Write description");
    }

    private async Task<Message> RequestPrice(ITelegramBotClient botClient, long chatId)
    {
        return await botClient.SendTextMessageAsync(chatId, "Enter the price");
    }

    private static async Task<Message> SendConfirmMessageAsync(ITelegramBotClient botClient, long chatId, string text,
        CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
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
            text: $"{text}. Can I save it?",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }
}

public class CategoryOptions
{
    public List<Category> Categories { get; private set; }
    public CategoryOptions(IConfiguration configuration)
    {
        Categories = configuration.GetSection("Categories").Get<List<Category>>();
    }
}