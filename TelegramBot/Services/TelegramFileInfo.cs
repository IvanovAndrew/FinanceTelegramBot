using Infrastructure;

namespace TelegramBot.Services;

public class TelegramFileInfo : ITelegramFileInfo
{
    public string FileId { get; init; } = "";
    public string? FileName { get; init; }
    public string? MimeType { get; init; }
    public IFile? Content { get; set; }
}