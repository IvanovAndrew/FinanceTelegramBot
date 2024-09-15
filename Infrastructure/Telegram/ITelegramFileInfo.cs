namespace Infrastructure.Telegram;

public interface ITelegramFileInfo
{
    string FileId { get; }
    string? FileName { get; }
    string? MimeType { get; }
    IFile? Content { get; set; }
}