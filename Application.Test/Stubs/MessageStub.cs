using Application;
using Infrastructure;
using Infrastructure.Telegram;

namespace UnitTest;

public class MessageStub : IMessage
{
    public int? Id { get; internal set; }
    public long ChatId { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; } = "";
    public bool Edited { get; internal set; }
    public MessageOptions? Options { get; internal set; }
    public Table? Table { get; internal set; }
    public IFileInfo? FileInfo { get; set; }
    public byte[]? PictureBytes { get; set; }
}