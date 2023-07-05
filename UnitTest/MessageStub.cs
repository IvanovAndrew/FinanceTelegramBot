using Infrastructure;

namespace UnitTest;

public class MessageStub : IMessage
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; }
    public bool Edited { get; }
    public TelegramKeyboard? TelegramKeyboard { get; set; }
}