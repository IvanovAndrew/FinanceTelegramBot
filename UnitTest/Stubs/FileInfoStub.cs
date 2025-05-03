using Application;
using Infrastructure;
using Infrastructure.Telegram;

namespace UnitTest;

public class FileInfoStub : IFileInfo
{
    public string FileId { get; set; } = "";
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public IFile? Content { get; set; }
}