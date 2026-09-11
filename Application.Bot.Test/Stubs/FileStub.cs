using Application.Bot;

namespace Application.Test.Stubs;

public class FileStub : IFile
{
    public string Text { get; init; } = "";
}