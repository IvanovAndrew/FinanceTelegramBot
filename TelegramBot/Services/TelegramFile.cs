using Infrastructure;

namespace TelegramBot.Services;

public class TelegramFile : IFile
{
    public string Text { get; init; } = "";
}