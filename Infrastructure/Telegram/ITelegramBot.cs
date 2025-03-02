namespace Infrastructure.Telegram;

public interface ITelegramBot
{
    Task<IMessage> SendTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default);
    Task<IMessage> EditSentTextMessageAsync(IMessageToSend messageToSend, CancellationToken cancellationToken = default);

    Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken);
    Task<IFile?> GetFileAsync(string fileId, string? mimeType, CancellationToken cancellationToken = default);

    Task<TelegramWebHookInfo> GetWebhookInfoAsync();
    Task SetWebhookAsync(string url);
}