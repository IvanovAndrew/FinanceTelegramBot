namespace Infrastructure;

public interface ITelegramBot
{
    Task<IMessage> SendTextMessageAsync(long chatId, string text);
    Task<IMessage> SendTextMessageAsync(long chatId, string text, TelegramKeyboard? keyboard = null, bool useMarkdown = false, CancellationToken cancellationToken = default);

    Task SetMyCommandsAsync(TelegramButton[] buttons, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken);
}