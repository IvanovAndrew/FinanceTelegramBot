using System.Text;
using Application;
using Domain;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;
using Message = Application.Message;

namespace Infrastructure.Telegram;

public class TelegramMessageService(ITelegramBotClient telegramBotClient)
    : IMessageService
{
    public async Task<int> SendTextMessageAsync(long chatId, string text, IReadOnlyCollection<Option>? options = null,
        Table? table = null, bool useMarkdown = false, CancellationToken cancellationToken = default)
    {
        InlineKeyboardMarkup? inlineKeyboard = null;
        if (options?.Any() == true)
        {
            inlineKeyboard = MapOptions(options);
        }

        var textToSend = text;
        if (table != null)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(FormatTable(table))}```";
            useMarkdown = true;
        }
        else if (useMarkdown)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(text)}```";
            useMarkdown = true;
        }

        var message = await telegramBotClient.SendMessage(
            chatId, 
            textToSend, 
            replyMarkup:inlineKeyboard, 
            parseMode: useMarkdown? ParseMode.MarkdownV2 : ParseMode.None,
            cancellationToken: cancellationToken);
        
        return message.Id;
    }

    public async Task<int> EditSentTextMessageAsync(long chatId, int messageId, string text, IReadOnlyCollection<Option>? options = null,
        Table? table = null, bool useMarkdown = false, CancellationToken cancellationToken = default)
    {
        InlineKeyboardMarkup? inlineKeyboard = null;
        if (options?.Any() == true)
        {
            inlineKeyboard = MapOptions(options);
        }

        string textToSend = text;
        if (table != null)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(FormatTable(table))}```";
            useMarkdown = true;
        }
        else if (useMarkdown)
        {
            textToSend = $"```{TelegramEscaper.EscapeString(text)}```";
            useMarkdown = true;
        }
        
        var message = await telegramBotClient.EditMessageText(
            chatId, 
            messageId, 
            textToSend,
            replyMarkup:inlineKeyboard,
            parseMode:useMarkdown? ParseMode.MarkdownV2: ParseMode.None,
            cancellationToken: cancellationToken);

        return message.Id;
    }

    private InlineKeyboardMarkup MapOptions(IReadOnlyCollection<Option> messageOptions)
    {
        var keyboardMarkup = new InlineKeyboardMarkup();

        foreach (var chunks in messageOptions.Chunk(3))
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

    public async Task<int> SendPictureAsync(long chatId, byte[] picture, string caption,
        CancellationToken cancellationToken = default)
    {
        if (picture is { } bytes)
        {
            using var stream = new MemoryStream(bytes);
            var message = await telegramBotClient.SendPhoto(chatId, new InputFileStream(stream), caption:caption, cancellationToken: cancellationToken);

            return message.Id;
        }

        throw new InvalidOperationException("No picture bytes found");
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
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(table.PostTableInfo);
            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    public async Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken)
    {
        // var diff = dateTimeService.Now().Subtract(message.Date);
        // if (diff.Hours >= 48)
        // {
        //     throw new DeleteOutdatedTelegramMessageException();
        // }

        try
        {
            await telegramBotClient.DeleteMessage(chatId, messageId, cancellationToken);
        }
        catch (Exception e)
        {
            throw new TelegramBotException(e);
        }
    }

    public async Task<IFile?> GetFileAsync(string fileId, CancellationToken cancellationToken)
    {
        var file = await telegramBotClient.GetFile(fileId, cancellationToken);

        if (file?.FilePath == null) return null;
        
        string text;
        using (var memoryStream = new MemoryStream())
        {
            await telegramBotClient.DownloadFile(file.FilePath, memoryStream, cancellationToken);
            var bytes = memoryStream.ToArray();
            text = Encoding.Default.GetString(bytes);
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