using Infrastructure;
using Infrastructure.Telegram;

namespace UnitTest;

public class FileStub : IFile
{
    public string Text { get; init; } = "";
}