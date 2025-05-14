using System.Text;
using Application;
using Domain;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;
using Message = Application.Message;

namespace Infrastructure.Telegram;

public class TelegramMessageService : IMessageService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IDateTimeService _dateTimeService;

    public TelegramMessageService(ITelegramBotClient telegramBotClient, IDateTimeService dateTimeService)
    {
        _telegramBotClient = telegramBotClient;
        _dateTimeService = dateTimeService;
    }
    
    public async Task<IMessage> SendTextMessageAsync(IMessage messageToSend, CancellationToken cancellationToken = default)
    {
        InlineKeyboardMarkup? inlineKeyboard = null;
        if (messageToSend.Options != null)
        {
            inlineKeyboard = MapOptions(messageToSend.Options);
        }

        var textToSend = messageToSend.Text;
        bool useMarkdown = false;
        if (messageToSend.Table != null)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(FormatTable(messageToSend.Table))}```";
            useMarkdown = true;
        }

        var message = await _telegramBotClient.SendMessage(
            messageToSend.ChatId, 
            textToSend, 
            replyMarkup:inlineKeyboard, 
            parseMode: useMarkdown? ParseMode.MarkdownV2 : ParseMode.None,
            cancellationToken: cancellationToken);
        
        return new Message(){Id = message.Id, ChatId = message.Chat.Id};
    }

    public async Task<IMessage> EditSentTextMessageAsync(IMessage messageToSend, CancellationToken cancellationToken = default)
    {
        if (messageToSend.Id == null)
        {
            return await SendTextMessageAsync(messageToSend, cancellationToken);
        }
        
        InlineKeyboardMarkup? inlineKeyboard = null;
        if (messageToSend.Options != null)
        {
            inlineKeyboard = MapOptions(messageToSend.Options);
        }

        string textToSend = messageToSend.Text;
        bool useMarkdown = false;
        if (messageToSend.Table != null)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(FormatTable(messageToSend.Table))}```";
            useMarkdown = true;
        }
        
        var message = await _telegramBotClient.EditMessageText(
            messageToSend.ChatId, 
            messageToSend.Id?? 0, 
            textToSend,
            replyMarkup:inlineKeyboard,
            parseMode:useMarkdown? ParseMode.MarkdownV2: ParseMode.None,
            cancellationToken: cancellationToken);

        return new Message(){Id = message.Id, ChatId = message.Chat.Id};
    }

    private string FormatTable(Table table)
    {
        int rowNamesLength = table.Rows.Max(row => row.FirstColumnValue.Length);
        int firstColumnLength = table.FirstColumnName.Length > rowNamesLength? table.FirstColumnName.Length : rowNamesLength;

        var currencyToLength = new Dictionary<Currency, int>();
        foreach (var currency in table.Currencies)
        {
            currencyToLength[currency] = 0;
        }

        foreach (var row in table.Rows)
        {
            foreach (var currency in table.Currencies)
            {
                if (row.CurrencyValues.Count > 0)
                {
                    var rowLength = row.CurrencyValues[currency].ToString().Length;
                    currencyToLength[currency] = Math.Max(rowLength, currencyToLength[currency]);
                }
            }
        }
        
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(table.Title);
        stringBuilder.AppendLine(table.Subtitle);
        stringBuilder.AppendLine();

        // column names
        stringBuilder.Append(table.FirstColumnName.PadLeft(firstColumnLength));
        
        foreach (var currency in table.Currencies)
        {
            stringBuilder.Append("|");
            stringBuilder.Append(currency.Name.PadLeft(currencyToLength[currency]));
        }

        stringBuilder.AppendLine();
        
        // ----- row
        stringBuilder.Append(new string('-', firstColumnLength));
        foreach (var currency in table.Currencies)
        {
            stringBuilder.Append("|");
            stringBuilder.Append(new string('-', currencyToLength[currency]));
        }

        stringBuilder.AppendLine();
        
        foreach (var row in table.Rows)
        {
            if (!string.IsNullOrEmpty(row.FirstColumnValue))
            {
                stringBuilder.Append(row.FirstColumnValue.PadLeft(firstColumnLength));
                foreach (var currencyName in table.ColumnNames)
                {
                    if (Currency.TryParse(currencyName, out var currency))
                    {
                        if (row.CurrencyValues.TryGetValue(currency, out var sum))
                        {
                            stringBuilder.Append("|");
                            stringBuilder.Append(sum.ToString().PadLeft(currencyToLength[currency]));
                        }
                    }
                    
                }
            }
            else
            {
                stringBuilder.Append(new string('-', firstColumnLength));
                foreach (var currency in table.Currencies)
                {
                    stringBuilder.Append("|");
                    stringBuilder.Append(new string('-', currencyToLength[currency]));
                }
            }

            stringBuilder.AppendLine();
        }

        if (!string.IsNullOrEmpty(table.PostTableInfo))
        {
            stringBuilder.AppendLine(table.PostTableInfo);
            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    public async Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        var diff = _dateTimeService.Now().Subtract(message.Date);
        if (diff.Hours >= 48)
        {
            throw new DeleteOutdatedTelegramMessageException();
        }

        try
        {
            await _telegramBotClient.DeleteMessage(message.ChatId, message.Id?? 0, cancellationToken);
        }
        catch (Exception e)
        {
            throw new TelegramBotException(e);
        }
    }

    public async Task<IFile?> GetFileAsync(string fileId, CancellationToken cancellationToken)
    {
        var file = await _telegramBotClient.GetFile(fileId, cancellationToken);

        if (file?.FilePath == null) return null;
        
        string text;
        using (var memoryStream = new MemoryStream())
        {
            await _telegramBotClient.DownloadFile(file.FilePath, memoryStream, cancellationToken);
            var bytes = memoryStream.ToArray();
            text = System.Text.Encoding.Default.GetString(bytes);
        }
        
        return new TelegramFile(){Text = text};
    }

    private InlineKeyboardMarkup MapOptions(MessageOptions messageOptions)
    {
        var keyboardMarkup = new InlineKeyboardMarkup();

        foreach (var chunks in messageOptions.Chunks())
        {
            keyboardMarkup.AddNewRow(chunks.Select(option => MapButton(option)).ToArray());
        }

        return keyboardMarkup;

        InlineKeyboardButton MapButton(Option option)
        {
            if (!string.IsNullOrEmpty(option.Code))
                return new InlineKeyboardButton(option.Text, option.Code);

            return new InlineKeyboardButton(option.Text);
        }
    }
}

public class TelegramBotException : Exception
{
    public TelegramBotException()
    {
        
    }
    
    public TelegramBotException(Exception exception) : base("Telegram client exception", exception)
    {
    }
}

public class TelegramBotSpecificException : TelegramBotException
{
    public TelegramBotSpecificException(Exception ex) : base(ex)
    {
    }
}

public class DeleteOutdatedTelegramMessageException : TelegramBotException
{
    public override string Message { get; } = "A message can only be deleted if it was sent less than 48 hours ago";
}