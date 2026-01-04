using Application;
using Infrastructure.Telegram;

namespace UnitTest;

public class MessageServiceMock : IMessageService
{
    private int _messageId = 0;
    private readonly List<MessageStub> _sentMessages = new();
    public IReadOnlyList<MessageStub> SentMessages => _sentMessages.AsReadOnly();
    public Dictionary<string, FileStub> SavedFiles = new();

    public Task<IMessage> SendTextMessageAsync(IMessage messageToSend, CancellationToken cancellationToken = default)
    {
        var message = new MessageStub()
        {
            Id = _messageId++,
            ChatId = messageToSend.ChatId,
            Options = messageToSend.Options,
            Table = messageToSend.Table,
            Text = messageToSend.Text,
            Date = DateTime.Now
        };
        
        _sentMessages.Add(message);
        return Task.FromResult<IMessage>(message);
    }

    public Task<IMessage> EditSentTextMessageAsync(IMessage messageToSend, CancellationToken cancellationToken = default)
    {
        var sentMessage = SentMessages.FirstOrDefault(m => m.ChatId == messageToSend.ChatId && m.Id == messageToSend.Id);

        if (sentMessage != null)
        {
            sentMessage.Text = messageToSend.Text;
            sentMessage.Options = messageToSend.Options;
            sentMessage.Table = messageToSend.Table;
        
            return Task.FromResult<IMessage>(sentMessage);
        }

        return SendTextMessageAsync(messageToSend, cancellationToken);
    }

    public Task SendPictureAsync(IMessage messageToSend, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        var messageToDelete = FindSentMessageById(message.ChatId, message.Id?? 0);
        if (messageToDelete != null)
        {
            _sentMessages.Remove(messageToDelete);
        }
        
        return Task.CompletedTask;
    }
    
    public Task<IFile?> GetFileAsync(string fileId, CancellationToken cancellationToken)
    {
        return Task.FromResult(SavedFiles[fileId] as IFile)!;
    }

    private MessageStub? FindSentMessageById(long chatId, int messageId)
    {
        return _sentMessages.FirstOrDefault(m => m.ChatId == chatId && m.Id == messageId);
    }
}