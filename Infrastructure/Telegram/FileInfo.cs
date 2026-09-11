using Application.Bot;

namespace Infrastructure.Telegram;

public class FileInfo : IFileInfo
{
    public string FileId { get; init; } = "";
    public string? FileName { get; init; }
    public string? MimeType { get; init; }
    public IFile? Content { get; set; }
}