using Infrastructure.Telegram;

namespace Application;

public interface IMessageService
{
    Task<int> SendTextMessageAsync(long chatId, string text, IReadOnlyCollection<Option>? options = null, Table? table = null, bool useMarkdown = false, CancellationToken cancellationToken = default);
    Task<int> EditSentTextMessageAsync(long chatId, int messageId, string text, IReadOnlyCollection<Option>? options = null, Table? table = null, bool useMarkdown = false, CancellationToken cancellationToken = default);
    Task<int> SendPictureAsync(long chatId, byte[] picture, string caption, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken);
    Task<IFile?> GetFileAsync(string fileId, CancellationToken cancellationToken = default);
}