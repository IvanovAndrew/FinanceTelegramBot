using Infrastructure.Telegram;

namespace Application;

public interface IMessageService
{
    Task<IMessage> SendTextMessageAsync(IMessage messageToSend, CancellationToken cancellationToken = default);
    Task<IMessage> EditSentTextMessageAsync(IMessage messageToSend, CancellationToken cancellationToken = default);
    Task SendPictureAsync(IMessage messageToSend, CancellationToken cancellationToken = default);
    Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken);
    Task<IFile?> GetFileAsync(string fileId, CancellationToken cancellationToken = default);
}