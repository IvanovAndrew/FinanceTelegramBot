using Application;

namespace UnitTest.Stubs;

public class MessageToSendStub : IMessage
{
    public int? Id { get; }
    public long ChatId { get; set; }
    public DateTime Date { get; }
    public string Text { get; set;}
    public bool Edited { get; }
    public MessageOptions? Options { get; }
    public Table? Table { get; }
    public IFileInfo? FileInfo { get; }
}