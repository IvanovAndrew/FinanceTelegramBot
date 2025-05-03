using Application;
using Infrastructure.Telegram;

namespace TelegramBot.Services;

public class FileInfo : IFileInfo
{
    public string FileId { get; init; } = "";
    public string? FileName { get; init; }
    public string? MimeType { get; init; }
    public IFile? Content { get; set; }
}