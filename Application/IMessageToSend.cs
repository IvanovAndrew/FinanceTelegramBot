using Infrastructure.Telegram;

namespace Application;

public interface IMessageToSend
{
    public long ChatId { get; }
    public int MessageId { get; set; }
    string Text { get; }
    bool UseMarkdown { get; }
    public bool IsEditable { get; }
    public bool IsRemovable { get; }
}