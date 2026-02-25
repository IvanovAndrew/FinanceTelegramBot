namespace Application.Test.Stubs;

public class MessageToSendStub : IMessage
{
    public int? Id { get; }
    public long ChatId { get; set; }
    public DateTime Date { get; }
    public string Text { get; set;} = String.Empty;
    public bool Edited { get; }
    public IReadOnlyCollection<Option>? Options { get; }
    public Table? Table { get; }
    public bool UseMarkdown { get; } = false;
    public IFileInfo? FileInfo { get; }
    public byte[]? PictureBytes { get; set; }
}