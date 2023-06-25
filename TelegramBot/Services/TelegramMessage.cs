﻿using Infrastructure;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Services;

public class TelegramMessage : IMessage
{
    public int Id { get; }
    public long ChatId { get; }
    public DateTime Date { get; }
    public string Text { get; }

    public TelegramMessage(Message message, string? text = null)
    {
        Id = message.MessageId;
        ChatId = message.Chat.Id;
        Date = message.Date;
        Text = text?? message.Text;
    }

    public static TelegramMessage FromUpdate(Update update)
    {
        if (update.Type == UpdateType.Message)
        {
            return new TelegramMessage(update.Message!);
        }
        if (update.Type == UpdateType.CallbackQuery)
        {
            return new TelegramMessage(update.CallbackQuery!.Message!, update.CallbackQuery.Data);
        }

        throw new ArgumentOutOfRangeException(
            $"Unexpected type of Update. Expected {UpdateType.Message} or {UpdateType.CallbackQuery} but {update.Type} was received");
    }
}