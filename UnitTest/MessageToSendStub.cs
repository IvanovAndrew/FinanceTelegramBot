using Infrastructure.Telegram;

namespace UnitTest;

public class MessageToSendStub : IMessageToSend
{
    public long ChatId { get; set; }
    public string Text { get; set;}
    public TelegramKeyboard? Keyboard { get; set;}
    public bool UseMarkdown { get; set;}
    public bool IsEditable { get; set;}
    public bool IsRemovable { get; }
    public int MessageId { get; set; }
}