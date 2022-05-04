using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace FinanсeBot
{
    public class FinanseBot
    {
        static readonly ITelegramBotClient bot = new TelegramBotClient(token);

        private const string token = "5348973824:AAGHVSS0CZ1g8N3cUjZQSkgn9S_yRCUOt3g";

        private static readonly ConcurrentDictionary<long, ExpenseBuilder> answers = new();

        private static readonly string[] categoryCodes =
        {
            Category.Clothes, Category.Education, Category.Events, Category.Food, Category.Gifts,
            Category.Hannah, Category.Health, Category.Hobbies, Category.Others, Category.Pets,
            Category.Psychologist, Category.Restaurants, Category.Services, Category.Transport
        };

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            Console.WriteLine(JsonConvert.SerializeObject(update));

            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    await SendGreetingInline(botClient: botClient, chatId: message.Chat.Id,
                        cancellationToken: cancellationToken);
                    return;
                }

                if (answers.IsEmpty) return;

                var builder = answers[message.From.Id];
                if (builder.Date == null)
                {
                    builder.Date = DateTime.Parse(message.Text);
                    string infoMessage = "Enter the category";
                    await botClient.SendTextMessageAsync(message.Chat, infoMessage);

                    await SendCategoriesInline(botClient, message.Chat.Id, cancellationToken);
                    return;
                }
                else if (builder.Sum == null)
                {
                    builder.Sum = decimal.Parse(message.Text);

                    string infoMessage =
                        $"Check your data: {builder.Date.Value:dd.MM.yyyy}, {builder.Category}, {builder.Sum}";
                    await SendConfirmMessageAsync(botClient, message.Chat.Id, cancellationToken);
                }
            }
            else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                string codeOfButton = update.CallbackQuery.Data;
                var message = update.CallbackQuery.Message;


                long fromId = update.CallbackQuery.From.Id;
                if (codeOfButton == "startExpense")
                {
                    answers[fromId] = new ExpenseBuilder();

                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Enter the date:",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }

                else if (categoryCodes.Any(c => c == codeOfButton))
                {
                    var builder = answers[fromId];
                    var category = message.ReplyMarkup.InlineKeyboard.SelectMany(c => c)
                        .First(c => c.CallbackData == codeOfButton);
                    builder.Category = category.Text;

                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Enter the price:",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
                
                else if (codeOfButton == "Save")
                {
                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Saved",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    answers.Remove(fromId, out var builder);
                }
                
                else if (codeOfButton == "Cancel")
                {
                    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "Canceled",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    answers.Remove(fromId, out var builder);
                }
            }

        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            // Some actions
            Console.WriteLine(JsonConvert.SerializeObject(exception));
            await Task.Delay(10);
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
                text: "What can I do for you?",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        private static async Task<Message> SendCategoriesInline(ITelegramBotClient botClient, long chatId,
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
                        InlineKeyboardButton.WithCallbackData(text: Category.Transport, callbackData: Category.Transport),
                        InlineKeyboardButton.WithCallbackData(text: Category.Food, callbackData: Category.Food),
                        InlineKeyboardButton.WithCallbackData(text: Category.Restaurants, callbackData: Category.Restaurants),
                        InlineKeyboardButton.WithCallbackData(text: Category.Gifts, callbackData: Category.Gifts),
                        
                    },
                    new[]
                    {
                        // second button in row
                        InlineKeyboardButton.WithCallbackData(text: Category.Services, callbackData: Category.Services),
                        InlineKeyboardButton.WithCallbackData(text: Category.Health, callbackData: Category.Health),
                        InlineKeyboardButton.WithCallbackData(text: Category.Clothes, callbackData: Category.Clothes),
                        InlineKeyboardButton.WithCallbackData(text: Category.Events, callbackData: Category.Events),
                        
                    },
                    new[]
                    {
                        // third button in row
                        InlineKeyboardButton.WithCallbackData(text: Category.Services, callbackData: Category.Services),
                        InlineKeyboardButton.WithCallbackData(text: Category.Hobbies, callbackData: Category.Hobbies),
                        InlineKeyboardButton.WithCallbackData(text: Category.Psychologist, callbackData: Category.Psychologist),
                        InlineKeyboardButton.WithCallbackData(text: Category.Education, callbackData: Category.Education),
                        
                    },
                    new []
                    {
                        // fourth button in row
                        InlineKeyboardButton.WithCallbackData(text: Category.Pets, callbackData: Category.Pets),
                        InlineKeyboardButton.WithCallbackData(text: Category.Hannah, callbackData: Category.Hannah),
                        InlineKeyboardButton.WithCallbackData(text: Category.Others, callbackData: Category.Others),
                    }
                });

            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Choose the category",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        private static async Task<Message> SendConfirmMessageAsync(ITelegramBotClient botClient, long chatId,
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
                text: "Can I save it?",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }

        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }

    internal class ExpenseBuilder
    {
        internal DateTime? Date { get; set; }
        internal string? Category { get; set; }
        internal string? SubCategory { get; set; }

        internal string? Description { get; set; }
        internal decimal? Sum { get; set; }

        public IExpense Build()
        {
            return new Expense
            {
                Date = Date.Value,
                Category = Category,
                SubCategory = SubCategory,
                Description = Description,
                Amount = Sum.Value,
            };
        }
    }
}
