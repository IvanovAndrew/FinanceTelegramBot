namespace Infrastructure.Telegram;

public class EditableMessageToSend : IMessageToSend
{
    public long ChatId { get; init;}
    public string Text { get; init;}
    public TelegramKeyboard? Keyboard { get; init; }
    public bool UseMarkdown { get; init; }

    public bool IsEditable => true;
    public bool IsRemovable => true;
    public int MessageId { get; set; }
}

public interface IMessageToSend
{
    public long ChatId { get; }
    public int MessageId { get; set; }
    string Text { get; }
    TelegramKeyboard? Keyboard { get; }
    bool UseMarkdown { get; }
    public bool IsEditable { get; }
    public bool IsRemovable { get; }
}