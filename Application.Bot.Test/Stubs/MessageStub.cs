using Application.Bot;

namespace Application.Test.Stubs;

public class IncomingMessageStub : IIncomingMessage
{
    public int? Id { get; internal set; }
    public long ChatId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; } = "";
    public bool Edited { get; internal set; }
    public IFileInfo? FileInfo { get; set; }
}

public class OutcomingMessageStub : IOutcomingMessage
{
    public int? Id { get; internal set; }
    public long ChatId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; } = "";
    public bool Edited { get; internal set; }
    public IReadOnlyCollection<Option>? Options { get; internal set; }
    public Table? Table { get; internal set; }
    public bool UseMarkdown { get; } = false;
    public IFileInfo? FileInfo { get; set; }
    public byte[]? PictureBytes { get; set; }
}