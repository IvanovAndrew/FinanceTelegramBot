namespace Infrastructure.Telegram;

public class NotEditableMessageToSend : IMessageToSend
{
    public long ChatId { get; init; }
    public string Text { get; init;}
    public TelegramKeyboard? Keyboard { get; init;}
    public bool UseMarkdown { get; init;}
    public bool IsEditable => false;
    public bool IsRemovable { get; init; }
    public int MessageId { get; set; }
}