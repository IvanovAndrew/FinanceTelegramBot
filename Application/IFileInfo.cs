using Infrastructure.Telegram;

namespace Application;

public interface IFileInfo
{
    string FileId { get; }
    string? FileName { get; }
    string? MimeType { get; }
    IFile? Content { get; set; }
}