﻿namespace Infrastructure;

public interface ITelegramBot
{
    Task<IMessage> SendTextMessageAsync(long chatId, string text);
    Task<IMessage> SendTextMessageAsync(long chatId, string? text = null, TelegramKeyboard? keyboard = null, bool useMarkdown = false, CancellationToken cancellationToken = default);

    Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken);
    Task<IFile?> GetFileAsync(string fileId, string? mimeType, CancellationToken cancellationToken = default);

    Task<TelegramWebHookInfo> GetWebhookInfoAsync();
    Task SetWebhookAsync(string url);
}