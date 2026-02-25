using Infrastructure.Telegram;
using UnitTest;

namespace Application.Test.Stubs;

public class MessageServiceMock : IMessageService
{
    private int _messageId = 0;
    private readonly List<MessageStub> _sentMessages = new();
    public IReadOnlyList<MessageStub> SentMessages => _sentMessages.AsReadOnly();
    public Dictionary<string, FileStub> SavedFiles = new();

    public Task<int> SendTextMessageAsync(long chatId, string text, IReadOnlyCollection<Option>? options = null,
        Table? table = null, bool useMarkdown = false, CancellationToken cancellationToken = default)
    {
        var message = new MessageStub()
        {
            Id = _messageId++,
            ChatId = chatId,
            Options = options,
            Table = table,
            Text = text,
            Date = DateTime.Now
        };
        
        _sentMessages.Add(message);
        return Task.FromResult<int>(message.Id.Value);
    }

    public Task<int> EditSentTextMessageAsync(long chatId, int messageId, string text, IReadOnlyCollection<Option>? options = null, Table? table = null, bool useMarkdown = false, CancellationToken cancellationToken = default)
    {
        var sentMessage = SentMessages.FirstOrDefault(m => m.ChatId == chatId && m.Id == messageId);

        if (sentMessage != null)
        {
            sentMessage.Text = text;
            sentMessage.Options = options;
            sentMessage.Table = table;
        
            return Task.FromResult<int>(sentMessage.Id.Value);
        }

        return SendTextMessageAsync(chatId, text, options, table, cancellationToken:cancellationToken);
    }

    public Task<int> SendPictureAsync(long chatId, byte[] picture, string caption,
        CancellationToken cancellationToken = default)
    {
        var message = new MessageStub()
        {
            Id = _messageId++,
            ChatId = chatId,
            Text = caption,
            Date = DateTime.Now,
            PictureBytes = picture
        };
        
        _sentMessages.Add(message);
        return Task.FromResult(message.Id.Value);
    }

    public Task DeleteMessageAsync(long chatId, int messageId, CancellationToken cancellationToken)
    {
        var messageToDelete = FindSentMessageById(chatId, messageId);
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